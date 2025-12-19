using Application.DTOs.Comment;
using Application.DTOs.Post;
using Domain.Entities;
using AutoMapper;

namespace Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Post mapping
        CreateMap<Post, PostDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count));

        // Comment recursive mapping - QUAN TRỌNG: tránh circular reference
        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => MapReplies(src, 0)))
            .ForMember(dest => dest.Depth, opt => opt.Ignore()); // Set manually

        // Flatten mapping
        CreateMap<Comment, CommentFlattenDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.Depth, opt => opt.Ignore())
            .ForMember(dest => dest.Path, opt => opt.Ignore());
    }

    // Helper để map recursive replies với depth tracking
    private static List<CommentDto> MapReplies(Comment comment, int depth)
    {
        var result = new List<CommentDto>();

        foreach (var reply in comment.Replies)
        {
            var replyDto = new CommentDto
            {
                Id = reply.Id,
                Content = reply.Content,
                UserId = reply.UserId,
                UserName = reply.User?.Name ?? "",
                PostId = reply.PostId,
                ParentCommentId = reply.ParentCommentId,
                Depth = depth + 1,
                Replies = MapReplies(reply, depth + 1) // Recursive
            };
            result.Add(replyDto);
        }

        return result;
    }
}