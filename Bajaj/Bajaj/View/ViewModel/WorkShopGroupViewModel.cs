using Bajaj.Model;
using Bajaj.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Bajaj.ViewModel
{
    public class WorkShopGroupViewModel : BaseViewModel
    {
        ApiServices services;
        public WorkShopGroupViewModel()
        {
            services = new ApiServices();
            WorkShopGroupList = new ObservableCollection<WorkShopGroupModel>();
            CityList = new ObservableCollection<WorkCity>();
            WorkShopList = new ObservableCollection<WorkShopGroup>();
            GetData();
            //{
            //    new WorkShopGroupModel
            //    {
            //        WorkShopGroup = "STC",
            //    },
            //    new WorkShopGroupModel
            //    {
            //        WorkShopGroup = "Eicher Non-Auto Application",
            //    },
            //    new WorkShopGroupModel
            //    {
            //        WorkShopGroup = "Export Dealers",
            //    },
            //    new WorkShopGroupModel
            //    {
            //        WorkShopGroup = "Domestic Dealers-Authorized",
            //    },
            //    new WorkShopGroupModel
            //    {
            //        WorkShopGroup = "Domestic Dealers-unauthorized",
            //    },
            //    new WorkShopGroupModel
            //    {
            //        WorkShopGroup = "VECV Plant",
            //    },
            //};
        }

        private ObservableCollection<WorkShopGroup> workShopList;
        public ObservableCollection<WorkShopGroup> WorkShopList
        {
            get => workShopList;
            set
            {
                workShopList = value;
                OnPropertyChanged("WorkShopList");
            }
        }

        private ObservableCollection<WorkCity> cityList;
        public ObservableCollection<WorkCity> CityList
        {
            get => cityList;
            set
            {
                cityList = value;
                OnPropertyChanged("CityList");
            }
        }

        private ObservableCollection<WorkShopGroupModel> workShopGroupList;
        public ObservableCollection<WorkShopGroupModel> WorkShopGroupList
        {
            get => workShopGroupList;
            set
            {
                workShopGroupList = value;
                OnPropertyChanged("WorkShopGroupList");
            }
        }

        public async void GetData()
        {
            try
            {
                var result = await services.GetWorkShopData();
                dynamic list = JsonConvert.DeserializeObject(result);
                var WorkShopGroup_List = list["workshops"];
                foreach (var WorkShopGroup in WorkShopGroup_List)
                {
                    //var InfoKey = pair.Name;
                    //var InfoValue = pair.Value;
                    foreach (var WorkShopCity in WorkShopGroup.Value)
                    {
                        foreach (var WorkShop in WorkShopCity)
                        {
                            foreach (var WorkShopone in WorkShop)
                            {
                                Model.WorkShopGroup shopGroup = new WorkShopGroup();
                                foreach (var WorkShoponee in WorkShopone)
                                {
                                    var InfoKey1 = WorkShoponee.Name;
                                    var InfoValue1 = WorkShoponee.Value;
                                    if (WorkShoponee.Name == "address")
                                    {
                                        shopGroup.address = WorkShoponee.Value;
                                    }
                                    if (WorkShoponee.Name == "city_id")
                                    {
                                        shopGroup.city_id = WorkShoponee.Value;
                                    }
                                    if (WorkShoponee.Name == "country_id")
                                    {
                                        shopGroup.country_id = WorkShoponee.Value;
                                    }
                                    if (WorkShoponee.Name == "created")
                                    {
                                        shopGroup.created = WorkShoponee.Value;
                                    }
                                    if (WorkShoponee.Name == "group_name_id")
                                    {
                                        shopGroup.group_name_id = WorkShoponee.Value;
                                    }
                                    if (WorkShoponee.Name == "id")
                                    {
                                        shopGroup.id = WorkShoponee.Value;
                                    }
                                    if (WorkShoponee.Name == "is_active")
                                    {
                                        shopGroup.is_active = WorkShoponee.Value;
                                    }
                                    if (WorkShoponee.Name == "modified")
                                    {
                                        shopGroup.modified = WorkShoponee.Value;
                                    }
                                    if (WorkShoponee.Name == "name")
                                    {
                                        shopGroup.name = WorkShoponee.Value;
                                    }
                                    if (WorkShoponee.Name == "name1")
                                    {
                                        shopGroup.name1 = WorkShoponee.Value;
                                    }
                                    if (WorkShoponee.Name == "oem_id")
                                    {
                                        shopGroup.oem_id = WorkShoponee.Value;
                                    }
                                    if (WorkShoponee.Name == "parent_id")
                                    {
                                        shopGroup.parent_id = WorkShoponee.Value;
                                    }
                                    if (WorkShoponee.Name == "pincode")
                                    {
                                        shopGroup.pincode = WorkShoponee.Value;
                                    }
                                    if (WorkShoponee.Name == "region")
                                    {
                                        shopGroup.region = WorkShoponee.Value;
                                    }
                                    if (WorkShoponee.Name == "state_id")
                                    {
                                        shopGroup.state_id = WorkShoponee.Value;
                                    }
                                    if (WorkShoponee.Name == "user_id")
                                    {
                                        shopGroup.user_id = WorkShoponee.Value;
                                    }
                                }
                                WorkShopList.Add(shopGroup);

                            }
                        }
                        CityList.Add(
                            new WorkCity
                            {
                                workshops = new List<WorkShopGroup>(WorkShopList.ToList()),
                                city = WorkShopCity.Name,
                            });
                        WorkShopList = new ObservableCollection<WorkShopGroup>();
                    }
                    if(CityList.Count > 0)
                    {
                        if (CityList.FirstOrDefault().workshops.FirstOrDefault().oem_id == App.selectedOem.id)
                        {
                            WorkShopGroupList.Add(
                            new WorkShopGroupModel
                            {
                                CityList = new List<WorkCity>(CityList.ToList()),
                                workshopsGroupName = WorkShopGroup.Name,
                            });
                        }
                    }
                    
                    

                    CityList = new ObservableCollection<WorkCity>();
                }

                //foreach(var item in result.Result.workshops)
                //{
                //    WorkShopGroupList.Add(
                //        new ShowWorkShopGroupClass
                //        {
                //            WorkShopGroup = item.Key
                //        });
                //}
            }
            catch (Exception ex)
            {
            }
        }
    }
}
