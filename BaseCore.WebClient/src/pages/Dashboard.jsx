import React, { useState, useEffect } from 'react';
import { dashboardApi } from '../services/api';
import { useAuth } from '../contexts/AuthContext';

const Dashboard = () => {
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const { user, loading: authLoading, isAdmin } = useAuth();

    // Chờ auth context load xong (từ localStorage) rồi mới fetch stats
    useEffect(() => {
        if (!authLoading) {
            loadStats();
        }
    }, [authLoading]);

    const loadStats = async () => {
        setLoading(true);
        setError(null);
        try {
            const res = await dashboardApi.getStats();
            setStats(res.data);
        } catch (err) {
            console.error('Failed to load stats:', err);
            if (err.response?.status === 401) {
                return; // Axios interceptor đã redirect về login
            }
            if (!err.response) {
                setError('Không thể kết nối API Gateway (port 5000). Hãy chắc chắn tất cả backend services đang chạy.');
            } else {
                setError(`Lỗi tải thống kê (${err.response.status}): ${err.response.data?.message || 'Lỗi server nội bộ'}`);
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="content-wrapper">
            <div className="content-header">
                <div className="container-fluid">
                    <div className="row mb-2">
                        <div className="col-sm-6">
                            <h1 className="m-0">Dashboard</h1>
                        </div>
                    </div>
                </div>
            </div>

            <section className="content">
                <div className="container-fluid">
                    {loading ? (
                        <div className="text-center py-5">
                            <div className="spinner-border text-primary" role="status">
                                <span className="sr-only">Loading...</span>
                            </div>
                        </div>
                    ) : error ? (
                        <div className="alert alert-danger">
                            <i className="fas fa-exclamation-triangle mr-2"></i>
                            {error}
                            <button className="btn btn-sm btn-outline-danger ml-3" onClick={loadStats}>
                                Thử lại
                            </button>
                        </div>
                    ) : (
                        <div className="row">
                            {/* Sản phẩm — hiện cho tất cả */}
                            <div className="col-lg-3 col-6">
                                <div className="small-box bg-info">
                                    <div className="inner">
                                        <h3>{stats?.totalProducts ?? 0}</h3>
                                        <p>Sản phẩm</p>
                                    </div>
                                    <div className="icon">
                                        <i className="fas fa-box"></i>
                                    </div>
                                    <a href="/products" className="small-box-footer">
                                        Xem thêm <i className="fas fa-arrow-circle-right"></i>
                                    </a>
                                </div>
                            </div>

                            {/* Danh mục — hiện cho tất cả */}
                            <div className="col-lg-3 col-6">
                                <div className="small-box bg-success">
                                    <div className="inner">
                                        <h3>{stats?.totalCategories ?? 0}</h3>
                                        <p>Danh mục</p>
                                    </div>
                                    <div className="icon">
                                        <i className="fas fa-tags"></i>
                                    </div>
                                    <a href="/categories" className="small-box-footer">
                                        Xem thêm <i className="fas fa-arrow-circle-right"></i>
                                    </a>
                                </div>
                            </div>

                            {/* Người dùng, Đơn hàng, Doanh thu — chỉ admin */}
                            {isAdmin() && (
                                <>
                                    <div className="col-lg-3 col-6">
                                        <div className="small-box bg-warning">
                                            <div className="inner">
                                                <h3>{stats?.totalUsers ?? 0}</h3>
                                                <p>Người dùng</p>
                                            </div>
                                            <div className="icon">
                                                <i className="fas fa-users"></i>
                                            </div>
                                            <a href="/users" className="small-box-footer">
                                                Quản lý <i className="fas fa-arrow-circle-right"></i>
                                            </a>
                                        </div>
                                    </div>

                                    <div className="col-lg-3 col-6">
                                        <div className="small-box bg-danger">
                                            <div className="inner">
                                                <h3>
                                                    {stats?.totalOrders ?? 0}
                                                    {(stats?.pendingOrders ?? 0) > 0 && (
                                                        <span
                                                            className="badge badge-light ml-2"
                                                            style={{ fontSize: '0.5em' }}
                                                        >
                                                            {stats.pendingOrders} chờ
                                                        </span>
                                                    )}
                                                </h3>
                                                <p>Đơn hàng</p>
                                            </div>
                                            <div className="icon">
                                                <i className="fas fa-shopping-cart"></i>
                                            </div>
                                            <a href="/orders" className="small-box-footer">
                                                Quản lý <i className="fas fa-arrow-circle-right"></i>
                                            </a>
                                        </div>
                                    </div>

                                    <div className="col-lg-3 col-6">
                                        <div className="small-box bg-teal">
                                            <div className="inner">
                                                <h3 style={{ fontSize: '1.8rem' }}>
                                                    {(stats?.totalRevenue ?? 0).toLocaleString('vi-VN')} đ
                                                </h3>
                                                <p>Doanh thu (Hoàn thành)</p>
                                            </div>
                                            <div className="icon">
                                                <i className="fas fa-chart-line"></i>
                                            </div>
                                            <a href="/orders" className="small-box-footer">
                                                Xem báo cáo <i className="fas fa-arrow-circle-right"></i>
                                            </a>
                                        </div>
                                    </div>

                                    {/* Hàng 2: chi tiết trạng thái đơn hàng */}
                                    <div className="col-12">
                                        <div className="card">
                                            <div className="card-header">
                                                <h3 className="card-title">Trạng thái đơn hàng</h3>
                                            </div>
                                            <div className="card-body p-0">
                                                <div className="row text-center" style={{ padding: '15px' }}>
                                                    <div className="col-4 col-md-2">
                                                        <span className="badge badge-warning p-2" style={{ fontSize: '1rem' }}>
                                                            {stats?.pendingOrders ?? 0}
                                                        </span>
                                                        <p className="mt-1 mb-0 text-muted small">Chờ duyệt</p>
                                                    </div>
                                                    <div className="col-4 col-md-2">
                                                        <span className="badge badge-info p-2" style={{ fontSize: '1rem' }}>
                                                            {stats?.processingOrders ?? 0}
                                                        </span>
                                                        <p className="mt-1 mb-0 text-muted small">Đang xử lý</p>
                                                    </div>
                                                    <div className="col-4 col-md-2">
                                                        <span className="badge badge-success p-2" style={{ fontSize: '1rem' }}>
                                                            {stats?.completedOrders ?? 0}
                                                        </span>
                                                        <p className="mt-1 mb-0 text-muted small">Hoàn thành</p>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </>
                            )}
                        </div>
                    )}

                    <div className="row">
                        <div className="col-12">
                            <div className="card">
                                <div className="card-header">
                                    <h3 className="card-title">Welcome to BaseCore Sales System</h3>
                                </div>
                                <div className="card-body">
                                    <p>This is a teaching framework for web development using:</p>
                                    <ul>
                                        <li><strong>Backend:</strong> .NET Core 8.0 with Entity Framework Core</li>
                                        <li><strong>Frontend:</strong> React 18 with React Router</li>
                                        <li><strong>UI:</strong> AdminLTE 3 with Bootstrap 4</li>
                                        <li><strong>Authentication:</strong> JWT Bearer Token</li>
                                    </ul>
                                    <p>Features include:</p>
                                    <ul>
                                        <li>User Authentication (Login/Logout)</li>
                                        <li>Product Management (CRUD with Search &amp; Pagination)</li>
                                        <li>Category Management</li>
                                        <li>Order Management with Approval Workflow</li>
                                        <li>User Management (Admin only)</li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
        </div>
    );
};

export default Dashboard;
