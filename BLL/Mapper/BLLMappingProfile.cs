using AutoMapper;
using BLL.ModelsDto;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace BLL.Mapper
{
    public class BLLMappingProfile : Profile
    {
        public BLLMappingProfile()
        {
            // From Entity to DTO
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.ProfileImage, opt => opt.MapFrom(src => src.Image));

            // From DTO to Entity
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.ProfileImage))
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // важно, иначе EF будет ругаться

            RoleMap();

            TagMap();

            Article();
            Comment();
        }

        private void Comment()
        {
            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<CommentDto, Comment>();
        }

        private void RoleMap()
        {
            CreateMap<IdentityRole, RoleDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            CreateMap<RoleDto, IdentityRole>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        }

        private void TagMap()
        {
            // Из Entity → в DTO
            CreateMap<Tag, TagDto>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.CreatedByUser.UserName));

            // Из DTO → в Entity
            CreateMap<TagDto, Tag>()
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore());
        }

        private void Article()
        {
            // Mapping из сущности в DTO
            CreateMap<Article, ArticleDto>()
    .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.Author.Id))
    .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.UserName))
    .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.ArticleTags.Select(at => at.Tag.Name)))
    .ForMember(dest => dest.TagsCount, opt => opt.MapFrom(src => src.ArticleTags.Count))
    .ForMember(dest => dest.CommentsCount, opt => opt.MapFrom(src => src.Comments.Count));

            // Mapping из DTO в сущность
            CreateMap<ArticleDto, Article>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.Ignore());
        }
    }
}