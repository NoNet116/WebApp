using AutoMapper;
using BLL.ModelsDto;
using WebApi.ViewModels;
using WebApi.ViewModels.Articles;
using WebApi.ViewModels.Tags;

namespace WebApi.Mapper
{
    public class PLLMappingProfile : Profile
    {
        public PLLMappingProfile()
        {
            CreateMap<UserViewModel, UserDto>().ReverseMap();
            CreateMap<RegisterUserModel, UserDto>().ReverseMap();

            RoleMap();
            TagMap();
            ArticleMap();
            CommentMap();
        }

        private void RoleMap()
        {
            CreateMap<RoleViewModel, RoleDto>().ReverseMap();

            CreateMap<RegisterRoleModel, RoleDto>();
        }

        private void TagMap()
        {
            CreateMap<TagViewModel, TagDto>().ReverseMap();
            CreateMap<RegisterTagModel, TagDto>().ReverseMap();
            CreateMap<UpdateViewModel, TagDto>().ReverseMap();
        }

        private void ArticleMap()
        {
            CreateMap<CreateArticleViewModel, ArticleDto>().ReverseMap();
            CreateMap<EditArticleViewModel, ArticleDto>().ReverseMap();
        }
        private void CommentMap()
        {
            CreateMap<CommentDto, ViewModels.Comments.Comment>().ReverseMap();
        }
    }
}