using SunnyRewards.Helios.Task.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel
{
    public class TenantTaskCategoryMockModel : TenantTaskCategoryModel
    {
        public TenantTaskCategoryMockModel()
        {
            TenantTaskCategoryId = 1;
            TaskCategoryId = 1;
            TenantCode = "tsk-210ddf7876234c11b64668d4246f0b44";
            ResourceJson = "Annual wellness visit";
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;


        }

        public List<TenantTaskCategoryModel> tenantcategory()
        {
            return new List<TenantTaskCategoryModel>()
             {
                 new TenantTaskCategoryMockModel()
             };
        }
    }
}
