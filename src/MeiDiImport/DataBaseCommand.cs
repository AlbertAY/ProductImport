using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace ClProductImport
{
    public class DataBaseCommand
    {

        //建立数据库连接
        public static SqlConnection GetSqlconnection()
        {
            string value = ConfigUtility.GetConnString("conn");
            SqlConnection conn = new SqlConnection(value);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// 保存产品信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static int SaveProduct(cl_Product model, SqlConnection conn, SqlTransaction transaction = null)
        {
            string sql = @"INSERT INTO [dbo].[cl_Product]
                                           ([ProductGUID]
                                           ,[ProductCode]
                                           ,[ProductName]
                                           ,[ProductSpec]
                                           ,[Unit]
                                           ,[Price]
                                           ,[PictureName]
                                           ,[Remarks]
                                           ,[BelongBUNameList]
                                           ,[ProductCodeFormat]
                                           ,[IsStrategy]
                                           ,[CreatedBy]
                                           ,[CreatedByGUID]
                                           ,[CreateTime]
                                           ,[LastMender]
                                           ,[LastMenderDate]
                                           ,[LastMenderGUID]
                                           ,[ProductTypeCode]
                                           ,[PutInStorageByGUID]
                                           ,[PutInStorageState]
                                           ,[QuotaAttributeNameList]
                                           ,[Source])
                                     VALUES
                                           (@ProductGUID
                                           ,@ProductCode
                                           ,@ProductName
                                           ,@ProductSpec
                                           ,@Unit
                                           ,@Price
                                           ,@PictureName
                                           ,@Remarks
                                           ,@BelongBUNameList
                                           ,@ProductCodeFormat
                                           ,@IsStrategy
                                           ,@CreatedBy
                                           ,@CreatedByGUID
                                           ,@CreateTime
                                           ,@LastMender
                                           ,@LastMenderDate
                                           ,@LastMenderGUID
                                           ,@ProductTypeCode
                                           ,@PutInStorageByGUID
                                           ,@PutInStorageState
                                           ,@QuotaAttributeNameList
                                           ,@Source)";

            return conn.Execute(sql, model, transaction);
        }


        /// <summary>
        /// 更新产品基础信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static int UpdateProduct(cl_Product model, SqlConnection conn, SqlTransaction transaction = null)
        {
            string sql = @"Update [dbo].[cl_Product]
                                  SET    [ProductName]=@ProductName
                                        ,[ProductSpec]=@ProductSpec
                                        ,[Unit]=@Unit
                                        ,[Remarks]=@Remarks
                                    WHERE ProductGUID=@ProductGUID";
            return conn.Execute(sql, model, transaction);
        }

        /// <summary>
        /// 保存产品扩展信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static int SaveProductExt(cl_Product_Ext model, SqlConnection conn, SqlTransaction transaction = null)
        {
            string sql = @"INSERT INTO [dbo].[cl_Product_Ext]
                                           ([ID]
                                           ,[ProductGUID]
                                           ,[brandName]
                                           ,[Specifications]
                                           ,[ProductModel]
                                           ,[YBFHQ]
                                           ,[DHHQ])
                                     VALUES
                                           (@ID
                                           ,@ProductGUID
                                           ,@brandName
                                           ,@Specifications
                                           ,@ProductModel
                                           ,@YBFHQ
                                           ,@DHHQ)";
            return conn.Execute(sql, model, transaction);
        }
        /// <summary>
        /// 更新产品扩展信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static int UpdateProductExt(cl_Product_Ext model, SqlConnection conn, SqlTransaction transaction = null)
        {
            string sql = @"Update [dbo].[cl_Product_Ext]
                                  SET brandName=@brandName
                                      ,[Specifications]=@Specifications
                                      ,[ProductModel]=@ProductModel
                                      ,[YBFHQ]=@YBFHQ
                                      ,[DHHQ]=@DHHQ
                                   WHERE  ProductGUID=@ProductGUID";
            return conn.Execute(sql, model, transaction);
        }
        /// <summary>
        /// 保存枚举指标属性值
        /// </summary>
        /// <param name="list"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static int SaveProduct2QuotaAttribute(List<p_Product2QuotaAttribute> list, SqlConnection conn, SqlTransaction transaction = null)
        {
            string sql = @"INSERT INTO [dbo].[p_Product2QuotaAttribute]
                                        ([Product2QuotaAttributeGUID]
                                        ,[ProductGUID]
                                        ,[ProductTypeQuotaGUID]
                                        ,[ProductTypeQuotaAttributeGUID])
                                    VALUES
                                        (@Product2QuotaAttributeGUID
                                        ,@ProductGUID
                                        ,@ProductTypeQuotaGUID
                                        ,@ProductTypeQuotaAttributeGUID)";
            return conn.Execute(sql, list, transaction);
        }
        /// <summary>
        /// 保存文本的指标属性值
        /// </summary>
        /// <param name="list"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static int SavaProductAttributeValue(List<p_ProductAttributeValue> list, SqlConnection conn, SqlTransaction transaction = null)
        {
            string sql = @"INSERT INTO [dbo].[p_ProductAttributeValue]
                                           ([ProductTypeQuotaAttributeExtGUID]
                                           ,[ProductGUID]
                                           ,[ProductTypeQuotaGUID]
                                           ,[ControlType]
                                           ,[QuotaAttributeValue])
                                     VALUES
                                           (@ProductTypeQuotaAttributeExtGUID
                                           ,@ProductGUID
                                           ,@ProductTypeQuotaGUID
                                           ,@ControlType
                                           ,@QuotaAttributeValue)";
            return conn.Execute(sql, list, transaction);
        }

        /// <summary>
        /// 产品在数据库中是否存在
        /// </summary>
        /// <param name="productCode"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static Guid HasProduct(string productCode, SqlConnection conn)
        {
            string sql = "SELECT ProductGUID FROM dbo.cl_Product WHERE ProductCode=@ProductCode";
            return conn.ExecuteScalar<Guid>(sql, new { ProductCode = productCode });
        }
        /// <summary>
        /// 根据分类名称获取分类的code
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static ProductType GetProductTypeCode(string typeName, SqlConnection conn)
        {
            string sql = "SELECT ProductTypeGUID,ProductTypeCode FROM dbo.p_ProductType WHERE ProductTypeShortName = @TypeName";
            return conn.QueryFirstOrDefault<ProductType>(sql, new { TypeName = typeName });
        }

        /// <summary>
        /// 获取指标属性相关信息
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static ProductIndex GetProductIndexEntity(string indexName, Guid refGUID, SqlConnection conn)
        {
            string sql = "SELECT ProductIndexGUID,ProductIndexCode,ControlType FROM p_ProductIndex WHERE RefGUID=@RefGUID AND ProductIndexShortName=@IndexName";
            return conn.QueryFirstOrDefault<ProductIndex>(sql, new { IndexName = indexName, RefGUID = refGUID });
        }
        /// <summary>
        /// 根据属性值名称获取属性值的guid
        /// </summary>
        /// <param name="attributeValue"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static string GetAttributeGUID(string attributeValue, Guid productTypeQuotaGUID, SqlConnection conn)
        {
            string sql = "SELECT ProductTypeQuotaAttributeGUID FROM p_ProductTypeQuotaAttribute WHERE ProductTypeQuotaGUID=@ProductTypeQuotaGUID AND QuotaAttributeValue=@AttributeValue";
            return conn.QueryFirstOrDefault<Guid>(sql, new { AttributeValue = attributeValue, ProductTypeQuotaGUID = productTypeQuotaGUID }).ToString();
        }

        public static bool HasUnit(string unit, SqlConnection conn)
        {
            string sql = "SELECT 1 FROM dbo.cl_Product_Unit WHERE Name=@Name";
            return conn.ExecuteScalar<string>(sql, new { Name = unit })?.Equals("1") ?? false;
        }


        public static bool UpdateCodeFormatInfo(string typeCode, Guid productGuid, SqlConnection conn, SqlTransaction sqlTransaction = null)
        {
            string[] info = typeCode.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (info == null || info.Length != 4)
            {
                throw new Exception("产品编码不标准");
            }
            string[] parameter = new string[3] { info[0], info[1], info[2] };
            string parameterCode = String.Join(".", parameter);
            string hasCodeInfo = @"SELECT 1 FROM cl_CodeFormatInfo WHERE Parameter=@Parameter";
            if (conn.ExecuteScalar<string>(hasCodeInfo, new { Parameter = parameterCode }, sqlTransaction)?.Equals("1") ?? false)//判断该分类的游标记录是否存在
            {
                string sql = @"UPDATE  dbo.cl_CodeFormatInfo
                            SET     FullCode = @FullCode ,
                                    SuffixCode = @SuffixCode,
                                    SourceID = @SourceID
                            WHERE   Parameter = @Parameter AND SuffixCode < @SuffixCode";

                return conn.Execute(sql, new { SourceID = productGuid, FullCode = typeCode, SuffixCode = info[3], Parameter = parameterCode }, sqlTransaction) > 0;
            }
            else
            {
                string sql = @"INSERT INTO dbo.cl_CodeFormatInfo
                            ( ID ,
                              SourceID ,
                              CodeType ,
                              FullCode ,
                              Parameter ,
                              SuffixCode
                            )
                    VALUES  ( @ID,
                              @SourceID,
                              '产品编码',
                              @FullCode,
                              @Parameter,
                              @SuffixCode
                            )";

                return conn.Execute(sql, new { ID = Guid.NewGuid(), SourceID = productGuid, FullCode = typeCode, SuffixCode = info[3], Parameter = parameterCode }, sqlTransaction) > 0;
            }
        }
        /// <summary>
        /// 获取产品指标属性的值是不是空的
        /// </summary>
        /// <param name="productGuid">产品guid</param>
        /// <returns></returns>
        public static string HasProductQuotaAttribute(Guid productGuid, Guid productTypeQuotaGUID, SqlConnection conn)
        {
            string sql = @"SELECT  ISNULL(pq.QuotaAttributeValue,'!!!!!!!!')
                            FROM p_Product2QuotaAttribute p2q
                                 LEFT JOIN p_ProductTypeQuotaAttribute pq ON pq.ProductTypeQuotaAttributeGUID = p2q.ProductTypeQuotaAttributeGUID
                            WHERE p2q.ProductGUID = @ProductGUID AND p2q.ProductTypeQuotaGUID=@ProductTypeQuotaGUID";
             return  conn.ExecuteScalar<string>(sql, new { ProductGUID = productGuid, ProductTypeQuotaGUID = productTypeQuotaGUID });            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productGuid"></param>
        /// <param name="productTypeQuotaGUID"></param>
        /// <param name="productIndexShortName"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static string ProductHasAttributeWenBen(Guid productGuid, Guid productTypeQuotaGUID, SqlConnection conn)
        {
            string sql = @"SELECT  ppv.QuotaAttributeValue
                            FROM    p_Product2QuotaAttribute p2q
                                    INNER JOIN p_ProductIndex pindex ON p2q.ProductTypeQuotaGUID = pindex.ProductIndexGUID
                                    LEFT JOIN p_ProductAttributeValue ppv ON p2q.ProductGUID = ppv.ProductGUID
                                                                             AND ppv.ProductTypeQuotaGUID = p2q.ProductTypeQuotaGUID
                            WHERE pindex.ControlType = '文本' AND p2q.ProductGUID=@ProductGUID 
                                   AND pindex.ProductIndexGUID =@ProductIndexGUID";
            return conn.ExecuteScalar<string>(sql, new { ProductGUID = productGuid, ProductIndexShortName = productTypeQuotaGUID,
                                                         ProductIndexGUID = productTypeQuotaGUID });
        }

        public static int UpdateProductAttributeWenBen(Guid productGuid, Guid productTypeQuotaGUID, string value,SqlConnection conn)
        {
            string sql = @"UPDATE p_ProductAttributeValue
                                SET QuotaAttributeValue=@QuotaAttributeValue
                                WHERE ProductGUID=@ProductGUID AND ProductTypeQuotaGUID=@ProductIndexGUID";
            return conn.Execute(sql, new { ProductGUID = productGuid, ProductIndexShortName = productTypeQuotaGUID,
                                                         ProductIndexGUID = productTypeQuotaGUID,
                                                         QuotaAttributeValue =value});
        }




        /// <summary>
        /// 重新挂关系
        /// </summary>
        /// <param name="productGuid">产品guid</param>
        /// <param name="attributeValue">属性值</param>
        /// <param name="conn"></param>
        /// <param name="sqlTransaction"></param>
        /// <returns></returns>
        public static int UpdateQuotaAttribute2Product(Guid productGuid, string attributeValue, SqlConnection conn)
        {
            string sql = @" UPDATE  p_Product2QuotaAttribute
                            SET     ProductTypeQuotaAttributeGUID = pq.ProductTypeQuotaAttributeGUID
                            FROM    p_Product2QuotaAttribute p2q
                                    INNER JOIN p_ProductTypeQuotaAttribute pq ON p2q.ProductTypeQuotaGUID = pq.ProductTypeQuotaGUID
                            WHERE   p2q.ProductGUID = @ProductGUID
                                    AND pq.QuotaAttributeValue = @QuotaAttributeValue";
            return conn.Execute(sql, new { ProductGUID = productGuid, QuotaAttributeValue = attributeValue });
        }
    }
}
