//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: global::Xamarin.Forms.Xaml.XamlResourceIdAttribute("Bajaj.View.PopupPages.ConfigureWifiDongle.xaml", "View/PopupPages/ConfigureWifiDongle.xaml", typeof(global::Bajaj.View.PopupPages.ConfigureWifiDongle))]

namespace Bajaj.View.PopupPages {
    
    
    [global::Xamarin.Forms.Xaml.XamlFilePathAttribute("View\\PopupPages\\ConfigureWifiDongle.xaml")]
    public partial class ConfigureWifiDongle : global::Rg.Plugins.Popup.Pages.PopupPage {
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private global::Bajaj.Controls.CustomEntry DeviceSSIDtxt;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private global::Bajaj.Controls.CustomEntry DevicePasswordtxt;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private global::Xamarin.Forms.Button BtnSubmit;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private global::Xamarin.Forms.Button BtnCancel;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private void InitializeComponent() {
            global::Xamarin.Forms.Xaml.Extensions.LoadFromXaml(this, typeof(ConfigureWifiDongle));
            DeviceSSIDtxt = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Bajaj.Controls.CustomEntry>(this, "DeviceSSIDtxt");
            DevicePasswordtxt = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Bajaj.Controls.CustomEntry>(this, "DevicePasswordtxt");
            BtnSubmit = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.Button>(this, "BtnSubmit");
            BtnCancel = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.Button>(this, "BtnCancel");
        }
    }
}