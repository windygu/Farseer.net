﻿using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FS.Core.Infrastructure;
using FS.Core.Set;
using FS.Mapping.Table;

namespace FS.Core.Client
{
    /// <summary>
    /// 数据库字段解析器总入口，根据要解析的类型，再分散到各自负责的解析器
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class ExpressionVisit<TEntity> where TEntity : class,new()
    {
        private readonly DbExpressionNewProvider<TEntity> _expFields;
        private readonly DbExpressionBoolProvider<TEntity> _expWhere;
        private readonly IQueue _queue;
        private readonly IQuery _query;

        /// <summary>
        /// 禁止无参数构造器
        /// </summary>
        private ExpressionVisit() { }

        /// <summary>
        /// 默认构造器
        /// </summary>
        /// <param name="query">数据库持久化</param>
        /// <param name="queue">每一次的数据库查询，将生成一个新的实例</param>
        /// <param name="expNewProvider">提供ExpressionNew表达式树的解析</param>
        /// <param name="expBoolProvider">提供ExpressionBinary表达式树的解析</param>
        public ExpressionVisit(IQuery query, IQueue queue, DbExpressionNewProvider<TEntity> expNewProvider, DbExpressionBoolProvider<TEntity> expBoolProvider)
        {
            _expFields = expNewProvider;
            _expWhere = expBoolProvider;
            _queue = queue;
            _query = query;
        }

        /// <summary>
        /// 赋值解析器
        /// </summary>
        /// <param name="entity">实体类</param>
        public string Assign(TEntity entity)
        {
            Clear();

            var map = TableMapCache.GetMap(entity);
            var sb = new StringBuilder();

            //  迭代实体赋值情况
            foreach (var kic in map.ModelList.Where(o => o.Value.IsDbField))
            {
                // 如果主键有值，则取消修改主键的SQL
                if (kic.Value.Column.IsDbGenerated) { continue; }
                var obj = kic.Key.GetValue(entity, null);
                if (obj == null || obj is TableSet<TEntity>) { continue; }

                //  查找组中是否存在已有的参数，有则直接取出
                var newParam = _query.DbProvider.CreateDbParam(_queue.Index + "_" + kic.Value.Column.Name, obj, _query.Param, _queue.Param);

                //  添加参数到列表
                sb.AppendFormat("{0} = {1} ,", _query.DbProvider.KeywordAegis(kic.Key.Name), newParam.ParameterName);
            }

            return sb.Length > 0 ? sb.Remove(sb.Length - 1, 1).ToString() : sb.ToString();
        }
        /// <summary>
        /// 赋值解析器
        /// </summary>
        /// <param name="exp">单个字段的赋值</param>
        public string Assign(Expression exp)
        {
            Clear();
            if (exp == null) { return null; }
            var sb = new StringBuilder();
            _expFields.Visit(exp, false);
            _expFields.SqlList.Reverse().ToList().ForEach(o => sb.Append(o + ","));
            return sb.Length > 0 ? sb.Remove(sb.Length - 1, 1).ToString() : sb.ToString();
        }
        /// <summary>
        /// 插入字段解析器
        /// </summary>
        /// <param name="entity">实体类</param>
        public string Insert(TEntity entity)
        {
            Clear();

            var map = TableMapCache.GetMap(entity);
            //  字段
            var strFields = new StringBuilder();
            //  值
            var strValues = new StringBuilder();

            //var lstParam = QueryQueue.Param;

            //  迭代实体赋值情况
            foreach (var kic in map.ModelList.Where(o => o.Value.IsDbField))
            {
                var obj = kic.Key.GetValue(entity, null);
                if (obj == null || obj is TableSet<TEntity>) { continue; }

                //  查找组中是否存在已有的参数，有则直接取出

                var newParam = _query.DbProvider.CreateDbParam(_queue.Index + "_" + kic.Value.Column.Name, obj, _query.Param, _queue.Param);

                //  添加参数到列表
                strFields.AppendFormat("{0},", _query.DbProvider.KeywordAegis(kic.Key.Name));
                strValues.AppendFormat("{0},", newParam.ParameterName);
            }
            //QueryQueue.Param = lstParam;
            return "(" + strFields.Remove(strFields.Length - 1, 1) + ") VALUES (" + strValues.Remove(strValues.Length - 1, 1) + ")";
        }
        /// <summary>
        /// 排序解析器
        /// </summary>
        /// <param name="lstExp">多个排序字段(true:正序；false：倒序）</param>
        /// <returns></returns>
        public string OrderBy(Dictionary<Expression, bool> lstExp)
        {
            if (lstExp == null || lstExp.Count == 0) { return null; }
            var sb = new StringBuilder();
            foreach (var keyValue in lstExp)
            {
                Clear();
                _expFields.Visit(keyValue.Key, false);
                _expFields.SqlList.Reverse().ToList().ForEach(o => sb.Append(o + ","));
                if (sb.Length <= 0) continue;
                sb = sb.Remove(sb.Length - 1, 1); sb.Append(string.Format(" {0}", keyValue.Value ? "ASC," : "DESC,"));
            }

            return sb.Length > 0 ? sb.Remove(sb.Length - 1, 1).ToString() : sb.ToString();
        }
        /// <summary>
        /// 字段筛选解析器
        /// </summary>
        /// <param name="lstExp">多个字段</param>
        /// <returns></returns>
        public string Select(List<Expression> lstExp)
        {
            Clear();
            if (lstExp == null || lstExp.Count == 0) { return null; }
            lstExp.ForEach(exp => _expFields.Visit(exp, true));

            var sb = new StringBuilder();
            _expFields.SqlList.Reverse().ToList().ForEach(o => sb.Append(o + ","));
            return sb.Length > 0 ? sb.Remove(sb.Length - 1, 1).ToString() : sb.ToString();
        }
        /// <summary>
        /// 条件解析器
        /// </summary>
        /// <param name="exp">条件</param>
        /// <returns></returns>
        public string Where(Expression exp)
        {
            _expWhere.Visit(exp);

            var sb = new StringBuilder();
            _expWhere.SqlList.Reverse().ToList().ForEach(o => sb.Append(o + ","));
            return sb.Length > 0 ? sb.Remove(sb.Length - 1, 1).ToString() : sb.ToString();
        }

        private void Clear()
        {
            _expFields.Clear();
            _expWhere.Clear();
        }
    }
}