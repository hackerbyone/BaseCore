import React, { useState, useEffect } from 'react';
import { orderApi } from '../services/api';

const statusConfig = {
    Pending:    { label: 'Chờ duyệt',   badge: 'warning' },
    Processing: { label: 'Đang xử lý',  badge: 'info' },
    Completed:  { label: 'Hoàn thành',  badge: 'success' },
    Cancelled:  { label: 'Đã hủy',      badge: 'secondary' },
    Rejected:   { label: 'Từ chối',     badge: 'danger' },
};

const Orders = () => {
    const [orders, setOrders] = useState([]);
    const [loading, setLoading] = useState(true);
    const [stats, setStats] = useState(null);
    const [statusFilter, setStatusFilter] = useState('');
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [selectedOrder, setSelectedOrder] = useState(null);
    const [orderDetail, setOrderDetail] = useState(null);
    const [showDetailModal, setShowDetailModal] = useState(false);
    const [actionLoading, setActionLoading] = useState(false);

    const pageSize = 10;

    useEffect(() => {
        loadOrders();
    }, [page, statusFilter]);

    useEffect(() => {
        loadStats();
    }, []);

    const loadOrders = async () => {
        setLoading(true);
        try {
            const params = { page, pageSize };
            if (statusFilter) params.status = statusFilter;
            const response = await orderApi.getAllOrders(params);
            setOrders(response.data.items || []);
            setTotalPages(response.data.totalPages || 0);
            setTotalCount(response.data.totalCount || 0);
        } catch (error) {
            console.error('Failed to load orders:', error);
        } finally {
            setLoading(false);
        }
    };

    const loadStats = async () => {
        try {
            const response = await orderApi.getStats();
            setStats(response.data);
        } catch (error) {
            console.error('Failed to load order stats:', error);
        }
    };

    const viewDetail = async (order) => {
        setSelectedOrder(order);
        setOrderDetail(null);
        setShowDetailModal(true);
        try {
            const response = await orderApi.getById(order.id);
            setOrderDetail(response.data);
        } catch (error) {
            console.error('Failed to load order detail:', error);
        }
    };

    const closeDetail = () => {
        setShowDetailModal(false);
        setSelectedOrder(null);
        setOrderDetail(null);
    };

    const handleApprove = async (orderId) => {
        if (!window.confirm('Duyệt đơn hàng này? Kho hàng sẽ bị trừ.')) return;
        setActionLoading(true);
        try {
            await orderApi.approve(orderId);
            await Promise.all([loadOrders(), loadStats()]);
        } catch (error) {
            alert(error.response?.data?.message || 'Duyệt đơn hàng thất bại');
        } finally {
            setActionLoading(false);
        }
    };

    const handleReject = async (orderId) => {
        const reason = window.prompt('Lý do từ chối (có thể để trống):');
        if (reason === null) return;
        setActionLoading(true);
        try {
            await orderApi.reject(orderId, reason);
            await Promise.all([loadOrders(), loadStats()]);
        } catch (error) {
            alert(error.response?.data?.message || 'Từ chối đơn hàng thất bại');
        } finally {
            setActionLoading(false);
        }
    };

    const handleComplete = async (orderId) => {
        if (!window.confirm('Xác nhận đơn hàng đã giao thành công?')) return;
        setActionLoading(true);
        try {
            await orderApi.updateStatus(orderId, 'Completed');
            await Promise.all([loadOrders(), loadStats()]);
        } catch (error) {
            alert(error.response?.data?.message || 'Cập nhật trạng thái thất bại');
        } finally {
            setActionLoading(false);
        }
    };

    const handleCancel = async (orderId) => {
        if (!window.confirm('Hủy đơn hàng này?')) return;
        setActionLoading(true);
        try {
            await orderApi.cancel(orderId);
            await Promise.all([loadOrders(), loadStats()]);
        } catch (error) {
            alert(error.response?.data?.message || 'Hủy đơn hàng thất bại');
        } finally {
            setActionLoading(false);
        }
    };

    const formatDate = (dateStr) => new Date(dateStr).toLocaleString('vi-VN');

    const formatCurrency = (amount) =>
        new Intl.NumberFormat('vi-VN').format(amount) + ' đ';

    const renderPagination = () => {
        const pages = [];
        for (let i = 1; i <= totalPages; i++) {
            pages.push(
                <li key={i} className={`page-item ${page === i ? 'active' : ''}`}>
                    <button className="page-link" onClick={() => setPage(i)}>{i}</button>
                </li>
            );
        }
        return pages;
    };

    return (
        <div className="content-wrapper">
            <div className="content-header">
                <div className="container-fluid">
                    <div className="row mb-2">
                        <div className="col-sm-6">
                            <h1 className="m-0">Quản lý đơn hàng</h1>
                        </div>
                    </div>
                </div>
            </div>

            <section className="content">
                <div className="container-fluid">

                    {/* Stats cards */}
                    {stats && (
                        <div className="row mb-3">
                            <div className="col-lg-2 col-md-4 col-6">
                                <div className="small-box bg-secondary">
                                    <div className="inner">
                                        <h3>{stats.totalOrders}</h3>
                                        <p>Tổng đơn</p>
                                    </div>
                                    <div className="icon"><i className="fas fa-list"></i></div>
                                </div>
                            </div>
                            <div className="col-lg-2 col-md-4 col-6">
                                <div className="small-box bg-warning">
                                    <div className="inner">
                                        <h3>{stats.pendingOrders}</h3>
                                        <p>Chờ duyệt</p>
                                    </div>
                                    <div className="icon"><i className="fas fa-clock"></i></div>
                                </div>
                            </div>
                            <div className="col-lg-2 col-md-4 col-6">
                                <div className="small-box bg-info">
                                    <div className="inner">
                                        <h3>{stats.processingOrders}</h3>
                                        <p>Đang xử lý</p>
                                    </div>
                                    <div className="icon"><i className="fas fa-truck"></i></div>
                                </div>
                            </div>
                            <div className="col-lg-2 col-md-4 col-6">
                                <div className="small-box bg-success">
                                    <div className="inner">
                                        <h3>{stats.completedOrders}</h3>
                                        <p>Hoàn thành</p>
                                    </div>
                                    <div className="icon"><i className="fas fa-check-circle"></i></div>
                                </div>
                            </div>
                            <div className="col-lg-4 col-md-8 col-12">
                                <div className="small-box bg-primary">
                                    <div className="inner">
                                        <h3 style={{ fontSize: '1.4rem' }}>{formatCurrency(stats.totalRevenue)}</h3>
                                        <p>Doanh thu (đã hoàn thành)</p>
                                    </div>
                                    <div className="icon"><i className="fas fa-dollar-sign"></i></div>
                                </div>
                            </div>
                        </div>
                    )}

                    <div className="card">
                        <div className="card-header">
                            <div className="row align-items-center">
                                <div className="col-md-6">
                                    <h3 className="card-title">Danh sách đơn hàng</h3>
                                </div>
                                <div className="col-md-6 text-right">
                                    <div className="form-inline justify-content-end">
                                        <label className="mr-2 mb-0">Trạng thái:</label>
                                        <select
                                            className="form-control form-control-sm"
                                            value={statusFilter}
                                            onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
                                        >
                                            <option value="">Tất cả</option>
                                            <option value="Pending">Chờ duyệt</option>
                                            <option value="Processing">Đang xử lý</option>
                                            <option value="Completed">Hoàn thành</option>
                                            <option value="Cancelled">Đã hủy</option>
                                            <option value="Rejected">Từ chối</option>
                                        </select>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="card-body p-0">
                            {loading ? (
                                <div className="text-center py-5">
                                    <div className="spinner-border text-primary" role="status">
                                        <span className="sr-only">Loading...</span>
                                    </div>
                                </div>
                            ) : (
                                <div className="table-responsive">
                                    <table className="table table-bordered table-striped table-sm mb-0">
                                        <thead className="thead-light">
                                            <tr>
                                                <th style={{ width: '70px' }}>Mã đơn</th>
                                                <th style={{ width: '150px' }}>Ngày đặt</th>
                                                <th>Địa chỉ giao</th>
                                                <th style={{ width: '130px' }}>Tổng tiền</th>
                                                <th style={{ width: '110px' }}>Trạng thái</th>
                                                <th style={{ width: '210px' }}>Thao tác</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {orders.length === 0 ? (
                                                <tr>
                                                    <td colSpan={6} className="text-center py-4">
                                                        Không có đơn hàng nào
                                                    </td>
                                                </tr>
                                            ) : (
                                                orders.map(order => {
                                                    const cfg = statusConfig[order.status] || { label: order.status, badge: 'secondary' };
                                                    return (
                                                        <tr key={order.id}>
                                                            <td className="text-center">#{order.id}</td>
                                                            <td>{formatDate(order.orderDate)}</td>
                                                            <td>{order.shippingAddress || <em className="text-muted">Không có</em>}</td>
                                                            <td className="text-right">{formatCurrency(order.totalAmount)}</td>
                                                            <td className="text-center">
                                                                <span className={`badge badge-${cfg.badge}`}>
                                                                    {cfg.label}
                                                                </span>
                                                            </td>
                                                            <td>
                                                                <button
                                                                    className="btn btn-xs btn-default mr-1"
                                                                    title="Xem chi tiết"
                                                                    onClick={() => viewDetail(order)}
                                                                >
                                                                    <i className="fas fa-eye"></i>
                                                                </button>
                                                                {order.status === 'Pending' && (
                                                                    <>
                                                                        <button
                                                                            className="btn btn-xs btn-success mr-1"
                                                                            onClick={() => handleApprove(order.id)}
                                                                            disabled={actionLoading}
                                                                            title="Duyệt đơn"
                                                                        >
                                                                            <i className="fas fa-check"></i> Duyệt
                                                                        </button>
                                                                        <button
                                                                            className="btn btn-xs btn-danger mr-1"
                                                                            onClick={() => handleReject(order.id)}
                                                                            disabled={actionLoading}
                                                                            title="Từ chối"
                                                                        >
                                                                            <i className="fas fa-times"></i> Từ chối
                                                                        </button>
                                                                    </>
                                                                )}
                                                                {order.status === 'Processing' && (
                                                                    <button
                                                                        className="btn btn-xs btn-primary mr-1"
                                                                        onClick={() => handleComplete(order.id)}
                                                                        disabled={actionLoading}
                                                                        title="Xác nhận hoàn thành"
                                                                    >
                                                                        <i className="fas fa-check-double"></i> Hoàn thành
                                                                    </button>
                                                                )}
                                                                {(order.status === 'Pending' || order.status === 'Processing') && (
                                                                    <button
                                                                        className="btn btn-xs btn-warning"
                                                                        onClick={() => handleCancel(order.id)}
                                                                        disabled={actionLoading}
                                                                        title="Hủy đơn"
                                                                    >
                                                                        <i className="fas fa-ban"></i> Hủy
                                                                    </button>
                                                                )}
                                                            </td>
                                                        </tr>
                                                    );
                                                })
                                            )}
                                        </tbody>
                                    </table>
                                </div>
                            )}
                        </div>

                        {!loading && (
                            <div className="card-footer">
                                <div className="d-flex justify-content-between align-items-center">
                                    <span className="text-muted">Tổng: <strong>{totalCount}</strong> đơn hàng</span>
                                    {totalPages > 1 && (
                                        <nav>
                                            <ul className="pagination pagination-sm mb-0">
                                                <li className={`page-item ${page === 1 ? 'disabled' : ''}`}>
                                                    <button className="page-link" onClick={() => setPage(page - 1)}>‹</button>
                                                </li>
                                                {renderPagination()}
                                                <li className={`page-item ${page === totalPages ? 'disabled' : ''}`}>
                                                    <button className="page-link" onClick={() => setPage(page + 1)}>›</button>
                                                </li>
                                            </ul>
                                        </nav>
                                    )}
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            </section>

            {/* Order Detail Modal */}
            {showDetailModal && (
                <>
                    <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1">
                        <div className="modal-dialog modal-lg">
                            <div className="modal-content">
                                <div className="modal-header bg-light">
                                    <h5 className="modal-title">
                                        <i className="fas fa-receipt mr-2"></i>
                                        Chi tiết đơn hàng #{selectedOrder?.id}
                                    </h5>
                                    <button type="button" className="close" onClick={closeDetail}>
                                        <span>&times;</span>
                                    </button>
                                </div>
                                <div className="modal-body">
                                    {!orderDetail ? (
                                        <div className="text-center py-4">
                                            <div className="spinner-border text-primary" role="status"></div>
                                        </div>
                                    ) : (
                                        <>
                                            <div className="row mb-3">
                                                <div className="col-md-4">
                                                    <small className="text-muted">Ngày đặt</small>
                                                    <div>{formatDate(orderDetail.order.orderDate)}</div>
                                                </div>
                                                <div className="col-md-4">
                                                    <small className="text-muted">Trạng thái</small>
                                                    <div>
                                                        <span className={`badge badge-${(statusConfig[orderDetail.order.status] || {}).badge || 'secondary'}`}>
                                                            {(statusConfig[orderDetail.order.status] || { label: orderDetail.order.status }).label}
                                                        </span>
                                                    </div>
                                                </div>
                                                <div className="col-md-4">
                                                    <small className="text-muted">Tổng tiền</small>
                                                    <div><strong>{formatCurrency(orderDetail.order.totalAmount)}</strong></div>
                                                </div>
                                                <div className="col-md-12 mt-2">
                                                    <small className="text-muted">Địa chỉ giao hàng</small>
                                                    <div>{orderDetail.order.shippingAddress || 'Không có'}</div>
                                                </div>
                                            </div>

                                            <table className="table table-bordered table-sm">
                                                <thead className="thead-light">
                                                    <tr>
                                                        <th>Sản phẩm</th>
                                                        <th className="text-right">Đơn giá</th>
                                                        <th className="text-center">Số lượng</th>
                                                        <th className="text-right">Thành tiền</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    {(orderDetail.details || []).map(item => (
                                                        <tr key={item.id}>
                                                            <td>{item.productName}</td>
                                                            <td className="text-right">{formatCurrency(item.unitPrice)}</td>
                                                            <td className="text-center">{item.quantity}</td>
                                                            <td className="text-right">{formatCurrency(item.unitPrice * item.quantity)}</td>
                                                        </tr>
                                                    ))}
                                                </tbody>
                                                <tfoot>
                                                    <tr>
                                                        <td colSpan={3} className="text-right font-weight-bold">Tổng cộng:</td>
                                                        <td className="text-right font-weight-bold">
                                                            {formatCurrency(orderDetail.order.totalAmount)}
                                                        </td>
                                                    </tr>
                                                </tfoot>
                                            </table>

                                            {/* Quick action buttons in modal */}
                                            {orderDetail.order.status === 'Pending' && (
                                                <div className="alert alert-warning d-flex justify-content-between align-items-center mb-0">
                                                    <span><i className="fas fa-clock mr-1"></i> Đơn hàng đang chờ duyệt</span>
                                                    <div>
                                                        <button
                                                            className="btn btn-sm btn-success mr-2"
                                                            onClick={() => { closeDetail(); handleApprove(orderDetail.order.id); }}
                                                            disabled={actionLoading}
                                                        >
                                                            <i className="fas fa-check mr-1"></i>Duyệt
                                                        </button>
                                                        <button
                                                            className="btn btn-sm btn-danger"
                                                            onClick={() => { closeDetail(); handleReject(orderDetail.order.id); }}
                                                            disabled={actionLoading}
                                                        >
                                                            <i className="fas fa-times mr-1"></i>Từ chối
                                                        </button>
                                                    </div>
                                                </div>
                                            )}
                                            {orderDetail.order.status === 'Processing' && (
                                                <div className="alert alert-info d-flex justify-content-between align-items-center mb-0">
                                                    <span><i className="fas fa-truck mr-1"></i> Đơn hàng đang được xử lý</span>
                                                    <button
                                                        className="btn btn-sm btn-primary"
                                                        onClick={() => { closeDetail(); handleComplete(orderDetail.order.id); }}
                                                        disabled={actionLoading}
                                                    >
                                                        <i className="fas fa-check-double mr-1"></i>Xác nhận hoàn thành
                                                    </button>
                                                </div>
                                            )}
                                        </>
                                    )}
                                </div>
                                <div className="modal-footer">
                                    <button type="button" className="btn btn-secondary" onClick={closeDetail}>
                                        Đóng
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div className="modal-backdrop fade show"></div>
                </>
            )}
        </div>
    );
};

export default Orders;
