using Bajaj.Model;
using Bajaj.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bajaj.ViewModel
{
    public class EcuViewModel : BaseViewModel
    {
        ApiServices services;
        public EcuViewModel()
        {
            services = new ApiServices();
            file = new List<Bajaj.Model.File>();
            ecu_map_file = new List<EcuMapFile>();
            flash_data = new Ecu2();
            SeedkeyalgoFnIndex_Value = new SeedkeyalgoFnIndex();
        }

        private List<Bajaj.Model.File> _file;
        public List<Bajaj.Model.File> file
        {
            get => _file;
            set
            {
                _file = value;
                OnPropertyChanged("file");
            }
        }

        private List<Bajaj.Model.File> _FileList;
        public List<Bajaj.Model.File> FileList
        {
            get => _FileList;
            set
            {
                _FileList = value;
            }
        }

        private Ecu2 _flash_data;
        public Ecu2 flash_data
        {
            get => _flash_data;
            set
            {
                _flash_data = value;
                OnPropertyChanged("flash_data");
            }
        }

        private SeedkeyalgoFnIndex _SeedkeyalgoFnIndex;
        public SeedkeyalgoFnIndex SeedkeyalgoFnIndex_Value
        {
            get => _SeedkeyalgoFnIndex;
            set
            {
                _SeedkeyalgoFnIndex = value;
                OnPropertyChanged("SeedkeyalgoFnIndex");
            }
        }

        private List<EcuMapFile> _ecu_map_file;
        public List<EcuMapFile> ecu_map_file
        {
            get => _ecu_map_file;
            set
            {
                _ecu_map_file = value;
                OnPropertyChanged("ecu_map_file");
            }
        }
        private PidCode _PIDParameterList;
        public PidCode PIDParameterList
        {
            get => _PIDParameterList;
            set
            {
                _PIDParameterList = value;
                OnPropertyChanged("PIDParameterList");
            }
        }
        public async Task get_models()
        {
            try
            {
                SubModel subModel = null;
                var model_detail = await services.get_all_models(App.JwtToken, 0);

                foreach (var model1 in model_detail.results.ToList())
                {
                    if (model1.sub_models != null && model1.sub_models.Count > 0)
                    {
                        foreach (var submodel in model1.sub_models.ToList())
                        {
                            if (submodel.id == App.sub_model_id)
                            {
                                subModel = submodel;
                            }
                        }
                    }
                }

                if(subModel!=null)
                {
                    var file_list = subModel.ecus.FirstOrDefault().ecu.FirstOrDefault().file;
                    file = file_list;

                    flash_data = subModel.ecus.FirstOrDefault().ecu.FirstOrDefault();

                    ecu_map_file = subModel.ecus.FirstOrDefault().ecu.FirstOrDefault().ecu_map_file;

                    SeedkeyalgoFnIndex_Value = subModel.ecus.FirstOrDefault().seedkeyalgo_fn_index;
                }

               

                App.is_login = false;
            }
            catch (Exception ex)
            {
            }
        }
        private File mFile;
        public File SelectFile
        {
            get { return mFile; }
            set
            {
                mFile = value;
                OnPropertyChanged("SelectFile");
                if (mFile != null)
                {
                    if (!CurrentUserEvent.Instance.IsExpert)
                    {
                        MessagingCenter.Send<File>((File)mFile, "ItemTapped");
                    }
                }

                // MessagingCenter.Send<JobcardViewModel>(this, "ItemClicked");
            }
        }
    }
}
