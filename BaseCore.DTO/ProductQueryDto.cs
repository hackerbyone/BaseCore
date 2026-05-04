namespace BaseCore.DTO.Product
{
    /// <summary>
    /// Tham số tìm kiếm / lọc sản phẩm từ client gửi lên
    /// </summary>
    public class ProductQueryDto
    {
        /// <summary>Từ khoá tìm theo tên sản phẩm</summary>
        public string? Keyword { get; set; }

        /// <summary>Lọc theo danh mục</summary>
        public int? CategoryId { get; set; }

        /// <summary>Giá tối thiểu</summary>
        public decimal? MinPrice { get; set; }

        /// <summary>Giá tối đa</summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>Thương hiệu</summary>
        public string? Brand { get; set; }

        /// <summary>Trang hiện tại (bắt đầu từ 1)</summary>
        public int Page { get; set; } = 1;

        /// <summary>Số sản phẩm mỗi trang</summary>
        public int PageSize { get; set; } = 16;
    }
}