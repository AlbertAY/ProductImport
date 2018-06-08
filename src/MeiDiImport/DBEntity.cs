using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClProductImport
{
    /// <summary>
    /// 产品信息表
    /// </summary>
    public class cl_Product
    {
        public Guid ProductGUID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductSpec { get; set; }
        public string Unit { get; set; }
        public double Price { get; set; }
        public string PictureName { get; set; }
        public string Remarks { get; set; }
        public string BelongBUNameList { get; set; }
        public string ProductCodeFormat { get; set; }
        public int IsStrategy { get; set; }
        public string CreatedBy { get; set; }
        public Guid CreatedByGUID { get; set; }
        public DateTime CreateTime { get; set; }
        public string LastMender { get; set; }
        public DateTime LastMenderDate { get; set; }
        public Guid LastMenderGUID { get; set; }
        public string ProductTypeCode { get; set; }
        public Guid ? PutInStorageByGUID { get; set; }
        public string PutInStorageState { get; set; }
        public string QuotaAttributeNameList { get; set; }
        public string Source { get; set; }
    }
    /// <summary>
    /// 产品表扩展表
    /// </summary>
    public class cl_Product_Ext
    {
        public Guid ID { get; set; }
        public Guid ProductGUID { get; set; }
        public string brandName { get; set; }
        public string Specifications { get; set; }
        public string ProductModel { get; set; }
        public int? YBFHQ { get; set; }
        public int? DHHQ { get; set; }
    }
    /// <summary>
    /// 枚举值保存
    /// </summary>
    public class p_Product2QuotaAttribute
    {
        public Guid Product2QuotaAttributeGUID { get; set; }
        public Guid ProductGUID { get; set; }
        public Guid ProductTypeQuotaGUID { get; set; }
        public Guid ProductTypeQuotaAttributeGUID { get; set; }

    }
    /// <summary>
    /// 指标属性文本保存
    /// </summary>
    public class p_ProductAttributeValue
    {
        public Guid ProductTypeQuotaAttributeExtGUID { get; set; }

        public Guid ProductGUID { get; set; }

        public Guid ProductTypeQuotaGUID { get; set; }

        public string ControlType { get; set; }

        public string QuotaAttributeValue { get; set; }
    }
}
