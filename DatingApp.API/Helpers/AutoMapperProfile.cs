using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Models;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserForListDto>().ForMember(dest => dest.PhotoUrl, opt =>
            {
                opt.MapFrom(src=> src.Photos.FirstOrDefault(p=> p.IsMain).Url);
            }).ForMember(dest => dest.Age, opt =>
            {
                opt.MapFrom(src => src.DateOfBirth.CalculateAge());
            });
            CreateMap<User, UserForDetailDto>().ForMember(dest => dest.PhotoUrl, opt =>
            {
                opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
            }).ForMember(dest => dest.Age, opt =>
            {
                opt.MapFrom(src => src.DateOfBirth.CalculateAge());
            });
            CreateMap<Photo, PhotoForDetailDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<PhotoForCreationDto, Photo>();
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<UserForRegisterDto, User>();
            CreateMap<MessageForCreationDto, Message>().ReverseMap();
            CreateMap<Message, MessageToReturnDto>().ForMember(dest => dest.SenderKnownAs, opt =>
            {
                opt.MapFrom(src => src.Sender.KnownAs);
            }).ForMember(dest => dest.RecipientKnownAs, opt =>
            {
                opt.MapFrom(src => src.Recipient.KnownAs);
            }).ForMember(dest => dest.SenderPhotoUrl, opt =>
            {
                opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(p => p.IsMain == true).Url);
            }).ForMember(dest => dest.RecipientPhotoUrl, opt =>
            {
                opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(p => p.IsMain == true).Url);
            });
        }
    }
}
