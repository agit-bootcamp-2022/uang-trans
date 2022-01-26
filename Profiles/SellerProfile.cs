using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using uang_trans.Input.Seller;
using uang_trans.Models;

namespace uang_trans.Profiles
{
    public class SellerProfile : Profile
    {
        public SellerProfile()
        {
            CreateMap<SellerCreateInput, Seller>();
        }
    }
}