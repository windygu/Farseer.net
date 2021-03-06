﻿using FS.Configs;
using FS.Core.Data;
using FS.Core.Infrastructure;
using FS.Core.Set;
using FS.Mapping.Table;

namespace FS.Core.Context
{
    /// <summary>
    /// 存储过程上下文
    /// </summary>
    public class ProcContext<TEntity> : DbContext where TEntity : class, new()
    {
        /// <summary>
        /// 通过TEntity的特性，获取数据库配置
        /// </summary>
        public ProcContext(string tableName = null) : this(TableMapCache.GetMap(typeof(ProcContext<TEntity>)).ClassInfo.ConnStr, TableMapCache.GetMap(typeof(ProcContext<TEntity>)).ClassInfo.DataType, TableMapCache.GetMap(typeof(ProcContext<TEntity>)).ClassInfo.CommandTimeout, tableName) { }

        /// <summary>
        /// 通过数据库配置，连接数据库
        /// </summary>
        /// <param name="dbIndex">数据库选项</param>
        /// <param name="tableName">表名称</param>
        public ProcContext(int dbIndex, string tableName = null) : this(DbFactory.CreateConnString(dbIndex), DbConfigs.ConfigInfo.DbList[dbIndex].DataType, DbConfigs.ConfigInfo.DbList[dbIndex].CommandTimeout, tableName) { }

        /// <summary>
        /// 通过自定义数据链接符，连接数据库
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="commandTimeout">SQL执行超时时间</param>
        /// <param name="tableName">表名称</param>
        public ProcContext(string connectionString, DataBaseType dbType = DataBaseType.SqlServer, int commandTimeout = 30, string tableName = null) : this(new DbExecutor(connectionString, dbType, commandTimeout), tableName) { }

        /// <summary>
        /// 事务
        /// </summary>
        /// <param name="database">数据库执行</param>
        /// <param name="tableName">表名称</param>
        public ProcContext(DbExecutor database, string tableName = null)
            : base(database, tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) { Name = TableMapCache.GetMap<TEntity>().ClassInfo.Name; }
            ProcSet = new ProcSet<TEntity>(this);
            Query = DbFactory.CreateQueryProc(this);
        }

        /// <summary>
        /// 数据库查询支持
        /// </summary>
        internal protected IQueryProc Query { get; set; }

        /// <summary>
        /// 强类型实体对象
        /// </summary>
        public ProcSet<TEntity> ProcSet { get; private set; }

        /// <summary>
        /// 提供快捷的数据库执行
        /// 根据实体类设置的特性，访问数据库
        /// </summary>
        public static ProcSet<TEntity> Data
        {
            get
            {
                return new ProcContext<TEntity>().ProcSet;
            }
        }
    }
}