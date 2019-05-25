using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeamChat.BL.Model;
using TeamChat.DAL.Entities;

namespace TeamChat.BL.Mappers
{


    internal static class ActivityMapper
    {
        internal static ActivityDetailModel MapToDetailModel(Activity activity)
        {
            if (activity.GetType() == typeof(Comment))
            {
                return CommentMapper.MapToDetailModel((Comment) activity);
            }

            if (activity.GetType() == typeof(Post))
            {
                return PostMapper.MapToListModel((Post) activity);
            }

            throw new Exception("Invalid activity entity to map!");
        }

        public static Activity MapToEntity(ActivityDetailModel activityDetailModel)
        {
            if (activityDetailModel.GetType() == typeof(CommentDetailModel))
            {
                return CommentMapper.MapToEntity((CommentDetailModel) activityDetailModel);
            }

            if (activityDetailModel.GetType() == typeof(PostListModel))
            {
                return PostMapper.MapListModelToEntity((PostListModel) activityDetailModel);
            }
            if (activityDetailModel.GetType() == typeof(PostDetailModel))
            {
                return PostMapper.MapDetailModelToEntity((PostDetailModel) activityDetailModel);
            }

            throw new Exception("Invalid activity model to map!");
        }

    }
}
