using Bajaj.Model;
using Bajaj.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bajaj.ViewModel
{
    public class LoginViewModel : BaseViewModel
    {
        ApiServices services;
        public LoginViewModel()
        {
            services = new ApiServices();
        }

        public LoginRespons LoginMethod(UserModel model)
        {
            try
            {
                var res = services.Login(model);
                //var pid_res = services.get_pid(App.JwtToken,6);
                return res.Result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Task<AllModelsModel> get_all_models(int id)
        {
            try
            {
                //var res = services.Login(model);
                var model = services.get_all_models(App.JwtToken, id);
                return model;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Results> get_pid(int id)
        {
            try
            {
                //var res = services.Login(model);
                var pids = await services.get_pid(App.JwtToken, id);
                return pids.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<DtcResults> get_dtc(int id)
        {
            try
            {
                //var res = services.Login(model);
                var dtcs = await services.get_dtc(App.JwtToken, id);
                return dtcs.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
