## 工具使用需要初始化两个参数配置，分别位于app.config的connectionStrings和appSettings节点
***
>1.配置connectionStrings的conn配置，该节点配置的是需要导入的数据库
   demo：
   
   ```<add name="conn" connectionString="Data Source=10.5.10.53\sql2005;Initial Catalog=dotnet_erp254_gzfjcszs_zsmd_Branch8_product_dev;User ID=sa;Password=95938"/> ```
   
   
>2.需要配置导入的execl的路径 也就是  appSettings下的filePath参数
demo:

```<add key="filePath" value="E:\ProductImport\src\file\ProductInfo.xlsx"/>```
