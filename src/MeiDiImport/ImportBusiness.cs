using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ClProductImport.WriteUtility;

namespace ClProductImport
{
    public class ImportBusiness
    {
        /// <summary>
        /// 导入数据开始
        /// </summary>
        /// <param name="table">需要导入的数据表格</param>
        /// <returns>返回导入失败的数据</returns>
        public static void ImportStart(string path)
        {
            DataTable table = ExcelUtility.ExcelToDataTable(path, true);
            DataColumn reslutColumn = new DataColumn("结果", typeof(string));
            table.Columns.Add(reslutColumn);
            if (table == null && (table?.Rows.Count ?? 0) > 0)
            {
                WriteConsoleAndLog("未获取到有效数据");
            }
            DataTable errorTable = table.Clone();//复制table架构
            DataTable successTable = table.Clone();
            DataTable updateTable = table.Clone();

            DataRowCollection rows = table.Rows;
            int columnsCount = table.Columns.Count;
            using (SqlConnection conn = DataBaseCommand.GetSqlconnection())
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    try
                    {
                        DataRow row = rows[i];
                        string msg = string.Empty;
                        bool updateDB = false;
                        if (InsertRow(row, columnsCount, conn, ref msg, ref updateDB))
                        {
                            WriteConsoleAndLog($"第{i}行导入成功");
                            DataRow successRow = successTable.NewRow();
                            successRow.ItemArray = (object[])row.ItemArray.Clone();
                            successRow["结果"] = "导入成功";//添加错误消息
                            successTable.Rows.Add(successRow);//添加到成功列表
                        }
                        else
                        {
                            if (updateDB)
                            {
                                WriteConsoleAndLog($"{msg}");
                                DataRow updateRow = updateTable.NewRow();
                                updateRow.ItemArray = (object[])row.ItemArray.Clone();
                                updateRow["结果"] = msg;//添加错误消息
                                updateTable.Rows.Add(updateRow);//添加到错误列表
                            }
                            else
                            {
                                WriteConsoleAndLog($"第{i}行导入失败，错误信息是{msg}");
                                DataRow errRow = errorTable.NewRow();
                                errRow.ItemArray = (object[])row.ItemArray.Clone();
                                errRow["结果"] = msg;//添加错误消息
                                errorTable.Rows.Add(errRow);//添加到错误列表
                            }


                        }
                    }
                    catch (Exception e)
                    {

                        throw;
                    }

                }
            }
            string dirPath = Path.GetDirectoryName(path); //获取路径地址
            dirPath = Path.Combine(dirPath, DateTime.Now.ToString("yyyyMMddHHmmss"));
            //保存结果到execl
            ExcelUtility.TableToExcel(errorTable, Path.Combine(dirPath, "错误.xlsx"));
            ExcelUtility.TableToExcel(successTable, Path.Combine(dirPath, "成功.xlsx"));
            ExcelUtility.TableToExcel(updateTable, Path.Combine(dirPath, "更新关系.xlsx"));
        }

        /// <summary>
        /// 行插入
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="columnsCount">总列数</param>
        /// <param name="msg">返回的处理消息</param>
        /// <returns></returns>
        public static bool InsertRow(DataRow row, int columnsCount, SqlConnection conn, ref string msg, ref bool updateDB)
        {
            bool hasProduct = false;
            //产品分类，材料编码，产品名称，单位
            if (string.IsNullOrEmpty(row[0].ToString()) ||
                string.IsNullOrEmpty(row[1].ToString()) ||
                string.IsNullOrEmpty(row[2].ToString()) ||
                string.IsNullOrEmpty(row[3].ToString()))
            {
                msg = "前4列参数是必填的！";
                return false;
            }
            ExeclRow productEntity = new ExeclRow();
            productEntity.TypeName = row[0].ToString();
            ProductType productType = DataBaseCommand.GetProductTypeCode(productEntity.TypeName, conn);
            if (productType == null || string.IsNullOrEmpty(productType.ProductTypeCode))
            {
                msg = $"产品分类[{productEntity.TypeName}]不存在";
                return false;
            }
            productEntity.TypeCode = productType.ProductTypeCode;
            productEntity.TypeGUID = productType.ProductTypeGUID;
            productEntity.Code = row[1].ToString();
            Guid productGuid = DataBaseCommand.HasProduct(productEntity.Code, conn);
            if (productGuid != Guid.Empty)
            {
                hasProduct = true;
                msg = $"产品编码为[{productEntity.Code}]已存在";
            }
            productEntity.Name = row[2].ToString();
            productEntity.Unit = row[3].ToString();
            if (!DataBaseCommand.HasUnit(productEntity.Unit, conn))
            {
                msg = $"单位[{productEntity.Unit}]不存在";
                return false;
            }
            productEntity.Remake = row[4].ToString();
            productEntity.BrandName = row[5].ToString();
            productEntity.ModelName = row[6].ToString();
            productEntity.Format = row[7].ToString();

            string strProductTime = row[8].ToString();
            strProductTime = strProductTime.Replace("天", "").Trim();
            int productTime = 0;
            if (!int.TryParse(strProductTime, out productTime))
            {
                msg = $"大货周期无法转换成整数";
                return false;
            }
            productEntity.ProductTime = productTime;

            string strExampleTime = row[9].ToString();
            strExampleTime = strExampleTime.Replace("天", "").Trim();
            int exampleTime = 0;
            if (!int.TryParse(strExampleTime, out exampleTime))
            {
                msg = $"样板房周期无法转换成整数！";
                return false;
            }
            productEntity.ExampleTime = exampleTime;

            productEntity.Attributes = new List<Attribute>();
            for (int i = 10; i < columnsCount; i++)
            {
                if (!string.IsNullOrEmpty(row[i].ToString()))
                {
                    Attribute attEntity = new Attribute();
                    string[] Attributes = row[i].ToString().Split(new char[] { ':', '：' }, StringSplitOptions.RemoveEmptyEntries);
                    if (Attributes == null || Attributes.Length != 2)
                    {
                        msg = $"指标属性[{row[i].ToString()}]异常";
                        return false;
                    }
                    else
                    {
                        Attributes[0] = Attributes[0].Replace("\n", "");
                        ProductIndex typeEntity = DataBaseCommand.GetProductIndexEntity(Attributes[0], productEntity.TypeGUID, conn);
                        if (typeEntity == null)
                        {
                            msg = $"指标属性[{Attributes[0]}]不存在";
                            return false;
                        }
                        //如果是枚举类型，还需要判断该枚举的属性值是否存在
                        if (typeEntity.ControlType == "枚举")
                        {
                            string ProductTypeQuotaAttributeGUID = DataBaseCommand.GetAttributeGUID(Attributes[1], typeEntity.ProductIndexGUID, conn);
                            if (string.IsNullOrEmpty(ProductTypeQuotaAttributeGUID) || ProductTypeQuotaAttributeGUID == "00000000-0000-0000-0000-000000000000")
                            {
                                msg = $"指标属性值[{Attributes[1]}]不存在";
                                return false;
                            }
                            attEntity.ProductTypeQuotaAttributeGUID = new Guid(ProductTypeQuotaAttributeGUID);
                        }
                        attEntity.ControlType = typeEntity.ControlType;
                        attEntity.ProductTypeQuotaGUID = typeEntity.ProductIndexGUID;
                        attEntity.Name = Attributes[0];
                        attEntity.Value = Attributes[1];
                        productEntity.Attributes.Add(attEntity);
                        productEntity.QuotaAttributeNameList += row[i] + ";";
                    }
                }
            }
            productEntity.QuotaAttributeNameList = productEntity.QuotaAttributeNameList?.TrimEnd(';') ?? string.Empty;//去掉拼接字符串最后面的分号
            if (hasProduct)
            {
                updateDB = UpdateDB(productEntity, productGuid, conn, ref msg);
                return false;
            }
            else
            {
                if (!InsertDB(productEntity, conn, ref msg))
                {
                    msg = "插入失败";
                    return false;
                }
                return true;
            }

        }

        public static bool InsertDB(ExeclRow entity, SqlConnection conn, ref string msg)
        {
            //基本信息
            cl_Product productModel = new cl_Product();
            productModel.ProductGUID = Guid.NewGuid();
            productModel.ProductCode = entity.Code;
            productModel.ProductName = entity.Name;
            productModel.ProductTypeCode = entity.TypeCode;
            productModel.Unit = entity.Unit;
            productModel.QuotaAttributeNameList = entity.QuotaAttributeNameList;
            productModel.Source = "工具导入";
            productModel.CreateTime = DateTime.Now;
            productModel.LastMenderDate = DateTime.Now;
            productModel.PutInStorageState = "已入库";
            productModel.Remarks = entity.Remake;
            //扩展信息
            cl_Product_Ext productExtModel = new cl_Product_Ext();
            productExtModel.ID = Guid.NewGuid();
            productExtModel.ProductGUID = productModel.ProductGUID;
            productExtModel.brandName = entity.BrandName;
            productExtModel.DHHQ = entity.ProductTime;
            productExtModel.YBFHQ = entity.ExampleTime;
            productExtModel.ProductModel = entity.ModelName;
            productExtModel.Specifications = entity.Format;
            //枚举信息
            Tuple<List<p_Product2QuotaAttribute>, List<p_ProductAttributeValue>> tupleResult = AttributeInsertList(entity, productModel);
            List<p_Product2QuotaAttribute> product2QuotaAttributeList = tupleResult.Item1;
            List<p_ProductAttributeValue> productAttributeValueList = tupleResult.Item2;

            using (SqlTransaction transaction = conn.BeginTransaction())
            {
                try
                {
                    DataBaseCommand.SaveProduct(productModel, conn, transaction);//保存产品
                    DataBaseCommand.SaveProductExt(productExtModel, conn, transaction);//保存扩展信息
                    DataBaseCommand.SaveProduct2QuotaAttribute(product2QuotaAttributeList, conn, transaction);//保存枚举值产品关系
                    DataBaseCommand.SavaProductAttributeValue(productAttributeValueList, conn, transaction);//保存文本信息
                    DataBaseCommand.UpdateCodeFormatInfo(productModel.ProductCode, productModel.ProductGUID, conn, transaction);
                    transaction.Commit();
                    msg = "成功";
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    msg = ex.Message;
                    Logger.GetLogger(ex.Message).Error(ex);
                    return false;
                }
            }
        }
        public static Tuple<List<p_Product2QuotaAttribute>, List<p_ProductAttributeValue>> AttributeInsertList(ExeclRow entity, cl_Product productModel)
        {
            //枚举信息
            List<p_Product2QuotaAttribute> product2QuotaAttributeList = new List<p_Product2QuotaAttribute>();
            List<p_ProductAttributeValue> productAttributeValueList = new List<p_ProductAttributeValue>();
            foreach (Attribute item in entity.Attributes)
            {
                if (item.ControlType == "枚举")
                {
                    p_Product2QuotaAttribute product2QuotaAttributeModel = new p_Product2QuotaAttribute();
                    product2QuotaAttributeModel.Product2QuotaAttributeGUID = Guid.NewGuid();
                    product2QuotaAttributeModel.ProductGUID = productModel.ProductGUID;
                    product2QuotaAttributeModel.ProductTypeQuotaGUID = item.ProductTypeQuotaGUID;
                    product2QuotaAttributeModel.ProductTypeQuotaAttributeGUID = item.ProductTypeQuotaAttributeGUID;
                    product2QuotaAttributeList.Add(product2QuotaAttributeModel);
                }
                else if (item.ControlType == "文本")
                {
                    p_ProductAttributeValue productAttributeValueModel = new p_ProductAttributeValue();
                    productAttributeValueModel.ProductTypeQuotaAttributeExtGUID = Guid.NewGuid();
                    productAttributeValueModel.ProductGUID = productModel.ProductGUID;
                    productAttributeValueModel.ProductTypeQuotaGUID = item.ProductTypeQuotaGUID;
                    productAttributeValueModel.ControlType = "2";
                    productAttributeValueModel.QuotaAttributeValue = item.Value;
                    productAttributeValueList.Add(productAttributeValueModel);
                    //文本的关系表绑定
                    p_Product2QuotaAttribute product2QuotaAttributeModel = new p_Product2QuotaAttribute();
                    product2QuotaAttributeModel.Product2QuotaAttributeGUID = Guid.NewGuid();
                    product2QuotaAttributeModel.ProductGUID = productModel.ProductGUID;
                    product2QuotaAttributeModel.ProductTypeQuotaGUID = item.ProductTypeQuotaGUID;
                    product2QuotaAttributeModel.ProductTypeQuotaAttributeGUID = productAttributeValueModel.ProductTypeQuotaAttributeExtGUID;//将文本分类的值关联到中间表
                    product2QuotaAttributeList.Add(product2QuotaAttributeModel);
                }
            }
            return Tuple.Create<List<p_Product2QuotaAttribute>, List<p_ProductAttributeValue>>(product2QuotaAttributeList, productAttributeValueList);

        }

        /// <summary>
        /// 如果产品存在就更新产品属性信息，为了修正之前修改分类指标属性导致的数据清空问题
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool UpdateDB(ExeclRow entity, Guid productGuid, SqlConnection conn, ref string msg)
        {
            int result = 0, addNew = 0;


            //枚举信息
            List<p_Product2QuotaAttribute> product2QuotaAttributeList = new List<p_Product2QuotaAttribute>();
            List<p_ProductAttributeValue> productAttributeValueList = new List<p_ProductAttributeValue>();
            foreach (Attribute item in entity.Attributes)
            {
                if (item.ControlType.Equals("枚举"))
                {
                    string value = DataBaseCommand.HasProductQuotaAttribute(productGuid, item.ProductTypeQuotaGUID, conn);
                    if (!value?.Equals(item.Value) ?? false)//之前指标属性导入后，更改属性值导致关系失效的错误数据修复
                    {
                        result += DataBaseCommand.UpdateQuotaAttribute2Product(productGuid, item.Value, conn);
                    }
                    else if (string.IsNullOrEmpty(value))//如果不存在关系，就说明该属性之前未导入
                    {

                        p_Product2QuotaAttribute product2QuotaAttributeModel = new p_Product2QuotaAttribute();
                        product2QuotaAttributeModel.Product2QuotaAttributeGUID = Guid.NewGuid();
                        product2QuotaAttributeModel.ProductGUID = productGuid;
                        product2QuotaAttributeModel.ProductTypeQuotaGUID = item.ProductTypeQuotaGUID;
                        product2QuotaAttributeModel.ProductTypeQuotaAttributeGUID = item.ProductTypeQuotaAttributeGUID;
                        product2QuotaAttributeList.Add(product2QuotaAttributeModel);

                    }
                }
                else if (item.ControlType.Equals("文本"))
                {
                    string value = DataBaseCommand.ProductHasAttributeWenBen(productGuid, item.ProductTypeQuotaGUID, conn);
                    if (string.IsNullOrEmpty(value))//文本类型的指标属性不存在
                    {
                        p_ProductAttributeValue productAttributeValueModel = new p_ProductAttributeValue();
                        productAttributeValueModel.ProductTypeQuotaAttributeExtGUID = Guid.NewGuid();
                        productAttributeValueModel.ProductGUID = productGuid;
                        productAttributeValueModel.ProductTypeQuotaGUID = item.ProductTypeQuotaGUID;
                        productAttributeValueModel.ControlType = "2";
                        productAttributeValueModel.QuotaAttributeValue = item.Value;
                        productAttributeValueList.Add(productAttributeValueModel);
                        //文本的关系表绑定
                        p_Product2QuotaAttribute product2QuotaAttributeModel = new p_Product2QuotaAttribute();
                        product2QuotaAttributeModel.Product2QuotaAttributeGUID = Guid.NewGuid();
                        product2QuotaAttributeModel.ProductGUID = productGuid;
                        product2QuotaAttributeModel.ProductTypeQuotaGUID = item.ProductTypeQuotaGUID;
                        product2QuotaAttributeModel.ProductTypeQuotaAttributeGUID = productAttributeValueModel.ProductTypeQuotaAttributeExtGUID;//将文本分类的值关联到中间表
                        product2QuotaAttributeList.Add(product2QuotaAttributeModel);
                    }
                    else if (!value.Equals(item.Value))
                    {
                        result += DataBaseCommand.UpdateProductAttributeWenBen(productGuid, item.ProductTypeQuotaGUID, item.Value, conn);
                    }
                }

            }
            using (SqlTransaction transaction = conn.BeginTransaction())
            {
                try
                {
                    addNew += DataBaseCommand.SaveProduct2QuotaAttribute(product2QuotaAttributeList, conn, transaction);//保存枚举值产品关系
                    addNew += DataBaseCommand.SavaProductAttributeValue(productAttributeValueList, conn, transaction);//保存文本信息
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    msg = ex.Message;
                    Logger.GetLogger(ex.Message).Error(ex);
                    return false;
                    throw ex;
                }
            }
            msg = "该产品已经已存在";
            if (result > 0)
            {
                msg += $",更新属性关系{result}次";
                return true;
            }
            if (addNew > 0)
            {
                msg += $",新增属性关系关系{addNew}次";
                return true;
            }
            msg += $"，并且指标属性值正常";
            return false;
        }
        /// <summary>
        /// 经过处理后的execl每行的数据对象
        /// </summary>
        public class ExeclRow
        {
            /// <summary>
            /// 分类名称
            /// </summary>
            public string TypeName { set; get; }
            /// <summary>
            /// 分类的code
            /// </summary>
            public string TypeCode { set; get; }
            /// <summary>
            /// 分类的guid
            /// </summary>
            public Guid TypeGUID { set; get; }
            /// <summary>
            /// 产品编码
            /// </summary>
            public string Code { get; set; }
            /// <summary>
            /// 产品名称
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 单位
            /// </summary>
            public string Unit { get; set; }
            /// <summary>
            /// 说明
            /// </summary>
            public string Remake { get; set; }
            /// <summary>
            /// 品牌名称
            /// </summary>
            public string BrandName { get; set; }
            /// <summary>
            /// 型号名称
            /// </summary>
            public string ModelName { get; set; }
            /// <summary>
            /// 规格
            /// </summary>
            public string Format { get; set; }
            /// <summary>
            /// 大货周期
            /// </summary>
            public int? ProductTime { get; set; }
            /// <summary>
            /// 样板房周期
            /// </summary>
            public int? ExampleTime { get; set; }
            /// <summary>
            /// 指标属性值冗余字段
            /// </summary>
            public string QuotaAttributeNameList { set; get; }
            /// <summary>
            /// 产品属性
            /// </summary>
            public List<Attribute> Attributes { set; get; }

        }
        /// <summary>
        /// 属性值
        /// </summary>
        public class Attribute
        {
            /// <summary>
            /// 属性名称
            /// </summary>
            public string Name { set; get; }
            /// <summary>
            /// 属性值
            /// </summary>
            public string Value { set; get; }
            /// <summary>
            /// 属性类型
            /// </summary>
            public string ControlType { set; get; }
            /// <summary>
            /// 指标guid
            /// </summary>
            public Guid ProductTypeQuotaGUID { set; get; }
            /// <summary>
            /// 属性值guid
            /// </summary>
            public Guid ProductTypeQuotaAttributeGUID { set; get; }

        }


    }
}
