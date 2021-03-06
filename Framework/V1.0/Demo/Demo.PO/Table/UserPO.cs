﻿using System.Data.Linq.Mapping;
using Demo.Common;
using FS.Core.Context;
using FS.Mapping.Table;

namespace Demo.PO.Table
{
    [DB(Name = "Members_User")]
    public class UserPO : TableContext<UserPO>
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Column(IsDbGenerated = true)]
        public int? ID { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord { get; set; }

        /// <summary>
        /// 会员类型
        /// </summary>
        public eumGenderType? GenderType { get; set; }

        /// <summary>
        /// 登陆次数
        /// </summary>
        public int? LoginCount { get; set; }

        /// <summary>
        /// 登陆IP
        /// </summary>
        public string LoginIP { get; set; }


        /// <summary>
        /// 登陆IP
        /// </summary>
        [Column(Name = "getdate()")]
        public string GetDate { get; set; }
    }
}