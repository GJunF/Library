using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Utils
{
    public class NoticeCheckerTest
    {

        public void Test1()
        {
            NoticeChecker<string> nc = new NoticeChecker<string>((invokeCount, str) =>
            {
                _Log.DebugFormat("{0}-{1}.", invokeCount, str);
                return false;
            }, 5, 5000, 2000);
            nc.Notice("test");
        }
        public void Test2()
        {
            NoticeChecker<ModifyRemarkRequest> nc = new NoticeChecker<ModifyRemarkRequest>(ModifyRemark, 5, 1000, 5000);
            nc.Notice(new ModifyRemarkRequest() { WechatFriendId = item.WechatFriendId, Remark = mark });
        }
        /// <summary>
        /// /api/ThirdParty/modifyRemark 修改好友备注
        /// </summary>
        /// <param name="request">修改备注请求</param>
        /// <returns>是否修改成功</returns>
        public bool ModifyRemark(int invokeCount, ModifyRemarkRequest request)
        {
            BaseResponse response = ModifyRemark(request.WechatFriendId, request.Remark);
            return response.Result == 0;
        }

    }
}
