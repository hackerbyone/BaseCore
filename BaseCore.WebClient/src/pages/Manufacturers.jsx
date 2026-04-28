import React, { useState, useEffect } from 'react';
import { manufacturerApi } from '../services/api';
import { useAuth } from '../contexts/AuthContext';

const Manufacturers = () => {
    const [manufacturers, setManufacturers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [keyword, setKeyword] = useState('');
    const [page, setPage] = useState(1);
    const [pageSize] = useState(10);
    const [totalPages, setTotalPages] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [showModal, setShowModal] = useState(false);
    const [editingManufacturer, setEditingManufacturer] = useState(null);
    const [formData, setFormData] = useState({ name: '', description: '', website: '', phone: '' });
    const [error, setError] = useState('');
    const { isAdmin } = useAuth();

    useEffect(() => { loadManufacturers(); }, [page, keyword]);

    const loadManufacturers = async () => {
        setLoading(true);
        try {
            const response = await manufacturerApi.getAll({ keyword: keyword || undefined, page, pageSize });
            setManufacturers(response.data.items || []);
            setTotalPages(response.data.totalPages || 0);
            setTotalCount(response.data.totalCount || 0);
        } catch (err) {
            console.error('Failed to load manufacturers:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleSearch = (e) => {
        e.preventDefault();
        setPage(1);
    };

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

    const openModal = (manufacturer = null) => {
        if (manufacturer) {
            setEditingManufacturer(manufacturer);
            setFormData({
                name: manufacturer.name,
                description: manufacturer.description || '',
                website: manufacturer.website || '',
                phone: manufacturer.phone || '',
            });
        } else {
            setEditingManufacturer(null);
            setFormData({ name: '', description: '', website: '', phone: '' });
        }
        setError('');
        setShowModal(true);
    };

    const closeModal = () => {
        setShowModal(false);
        setEditingManufacturer(null);
        setError('');
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        try {
            if (editingManufacturer) {
                await manufacturerApi.update(editingManufacturer.id, formData);
                closeModal();
                loadManufacturers();
            } else {
                await manufacturerApi.create(formData);
                closeModal();
                setPage(1);
            }
        } catch (err) {
            setError(err.response?.data?.message || 'Thao tác thất bại');
        }
    };

    const handleDelete = async (id) => {
        if (!window.confirm('Bạn có chắc muốn xóa nhà sản xuất này?')) return;
        try {
            await manufacturerApi.delete(id);
            if (manufacturers.length === 1 && page > 1) {
                setPage(page - 1);
            } else {
                loadManufacturers();
            }
        } catch (err) {
            alert(err.response?.data?.message || 'Xóa nhà sản xuất thất bại');
        }
    };

    return (
        <div className="content-wrapper">
            <div className="content-header">
                <div className="container-fluid">
                    <div className="row mb-2">
                        <div className="col-sm-6">
                            <h1 className="m-0">Quản lý Nhà sản xuất</h1>
                        </div>
                    </div>
                </div>
            </div>

            <section className="content">
                <div className="container-fluid">
                    <div className="card">
                        <div className="card-header">
                            <div className="row align-items-center">
                                <div className="col-md-4">
                                    <h3 className="card-title mb-0">Danh sách nhà sản xuất</h3>
                                </div>
                                <div className="col-md-5">
                                    <form onSubmit={handleSearch} className="d-flex">
                                        <input
                                            type="text"
                                            className="form-control form-control-sm mr-2"
                                            placeholder="Tìm theo tên..."
                                            value={keyword}
                                            onChange={(e) => setKeyword(e.target.value)}
                                        />
                                        <button type="submit" className="btn btn-sm btn-primary">
                                            <i className="fas fa-search"></i>
                                        </button>
                                    </form>
                                </div>
                                <div className="col-md-3 text-right">
                                    {isAdmin() && (
                                        <button className="btn btn-success btn-sm" onClick={() => openModal()}>
                                            <i className="fas fa-plus"></i> Thêm nhà sản xuất
                                        </button>
                                    )}
                                </div>
                            </div>
                        </div>
                        <div className="card-body">
                            {loading ? (
                                <div className="text-center py-5">
                                    <div className="spinner-border text-primary"></div>
                                </div>
                            ) : (
                                <>
                                <table className="table table-bordered table-striped">
                                    <thead>
                                        <tr>
                                            <th style={{ width: '60px' }}>ID</th>
                                            <th>Tên</th>
                                            <th>Mô tả</th>
                                            <th>Website</th>
                                            <th>Điện thoại</th>
                                            {isAdmin() && <th style={{ width: '120px' }}>Thao tác</th>}
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {manufacturers.length === 0 ? (
                                            <tr>
                                                <td colSpan={isAdmin() ? 6 : 5} className="text-center text-muted">
                                                    Không có nhà sản xuất nào
                                                </td>
                                            </tr>
                                        ) : (
                                            manufacturers.map(m => (
                                                <tr key={m.id}>
                                                    <td>{m.id}</td>
                                                    <td><strong>{m.name}</strong></td>
                                                    <td>{m.description}</td>
                                                    <td>
                                                        {m.website && (
                                                            <a href={m.website} target="_blank" rel="noreferrer">
                                                                {m.website}
                                                            </a>
                                                        )}
                                                    </td>
                                                    <td>{m.phone}</td>
                                                    {isAdmin() && (
                                                        <td>
                                                            <button
                                                                className="btn btn-sm btn-info mr-1"
                                                                onClick={() => openModal(m)}
                                                            >
                                                                <i className="fas fa-edit"></i>
                                                            </button>
                                                            <button
                                                                className="btn btn-sm btn-danger"
                                                                onClick={() => handleDelete(m.id)}
                                                            >
                                                                <i className="fas fa-trash"></i>
                                                            </button>
                                                        </td>
                                                    )}
                                                </tr>
                                            ))
                                        )}
                                    </tbody>
                                </table>

                                <div className="d-flex justify-content-between align-items-center mt-2">
                                    <span>Tổng cộng: {totalCount} nhà sản xuất</span>
                                    <nav>
                                        <ul className="pagination mb-0">
                                            <li className={`page-item ${page === 1 ? 'disabled' : ''}`}>
                                                <button className="page-link" onClick={() => setPage(page - 1)}>
                                                    Trước
                                                </button>
                                            </li>
                                            {renderPagination()}
                                            <li className={`page-item ${page === totalPages || totalPages === 0 ? 'disabled' : ''}`}>
                                                <button className="page-link" onClick={() => setPage(page + 1)}>
                                                    Sau
                                                </button>
                                            </li>
                                        </ul>
                                    </nav>
                                </div>
                                </>
                            )}
                        </div>
                    </div>
                </div>
            </section>

            {showModal && (
                <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1">
                    <div className="modal-dialog">
                        <div className="modal-content">
                            <div className="modal-header">
                                <h5 className="modal-title">
                                    {editingManufacturer ? 'Sửa nhà sản xuất' : 'Thêm nhà sản xuất'}
                                </h5>
                                <button type="button" className="close" onClick={closeModal}>
                                    <span>&times;</span>
                                </button>
                            </div>
                            <form onSubmit={handleSubmit}>
                                <div className="modal-body">
                                    {error && <div className="alert alert-danger">{error}</div>}
                                    <div className="form-group">
                                        <label>Tên nhà sản xuất <span className="text-danger">*</span></label>
                                        <input
                                            type="text"
                                            className="form-control"
                                            value={formData.name}
                                            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                            required
                                        />
                                    </div>
                                    <div className="form-group">
                                        <label>Mô tả</label>
                                        <textarea
                                            className="form-control"
                                            value={formData.description}
                                            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                                            rows="3"
                                        />
                                    </div>
                                    <div className="form-group">
                                        <label>Website</label>
                                        <input
                                            type="text"
                                            className="form-control"
                                            placeholder="https://..."
                                            value={formData.website}
                                            onChange={(e) => setFormData({ ...formData, website: e.target.value })}
                                        />
                                    </div>
                                    <div className="form-group">
                                        <label>Điện thoại</label>
                                        <input
                                            type="text"
                                            className="form-control"
                                            value={formData.phone}
                                            onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                                        />
                                    </div>
                                </div>
                                <div className="modal-footer">
                                    <button type="button" className="btn btn-secondary" onClick={closeModal}>
                                        Hủy
                                    </button>
                                    <button type="submit" className="btn btn-primary">
                                        {editingManufacturer ? 'Cập nhật' : 'Tạo mới'}
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            )}
            {showModal && <div className="modal-backdrop fade show"></div>}
        </div>
    );
};

export default Manufacturers;
