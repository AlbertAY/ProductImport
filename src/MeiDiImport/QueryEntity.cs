using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClProductImport
{
    public class ProductIndex
    {
        /// <summary>
        /// 属性guid
        /// </summary>
        public Guid ProductIndexGUID { set; get; }
        /// <summary>
        /// 属性code
        /// </summary>
        public string ProductIndexCode { set; get; }
        /// <summary>
        /// 属性类型
        /// </summary>
        public string ControlType { set; get; }

    }

    public class ProductType
    {
        /// <summary>
        /// 分类code
        /// </summary>
        public string ProductTypeCode { set; get; }
        /// <summary>
        /// 分类guid
        /// </summary>
        public Guid ProductTypeGUID { set; get; }
    }
}
