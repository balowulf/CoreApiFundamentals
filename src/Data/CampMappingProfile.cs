using AutoMapper;
using CoreCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Data
{
    public class CampMappingProfile : Profile
    {
        public CampMappingProfile()
        {
            this.CreateMap<Camp, CampModel>()
                .ForMember(camp => camp.Venue, opt => opt.MapFrom(src => src.Location.VenueName))
                .ReverseMap();
            this.CreateMap<Talk, TalkModel>();
            this.CreateMap<Speaker, SpeakerModel>();
        }
    }
}
