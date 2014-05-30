using System;
using System.Configuration;
using System.Web.Configuration;

namespace Catalogue.Web.Code
{
    public interface IEnvironment
    {
        string Name { get; }
        bool WindowsAuthentication { get; }
    }

    public class Environment : IEnvironment
    {
        public string Name
        {
            get { return ConfigurationManager.AppSettings["Environment"]; }
        }

        public bool WindowsAuthentication
        {
            get
            {
                var configuration = WebConfigurationManager.OpenWebConfiguration("/");
                var authenticationSection = (AuthenticationSection)configuration.GetSection("system.web/authentication");
                return authenticationSection.Mode == AuthenticationMode.Windows;
            }
        }
    }
}
