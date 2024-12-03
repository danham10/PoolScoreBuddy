//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PoolScoreBuddy.Platforms.Android
//{

//    //https://stackoverflow.com/questions/71047509/trust-anchor-for-certification-path-not-found-in-a-net-maui-project-trying-t
//    internal class InsecureHttpHandler
//    {
//        // This method must be in a class in a platform project, even if
//        // the HttpClient object is constructed in a shared project.
//        public HttpClientHandler GetInsecureHandler()
//        {
//            HttpClientHandler handler = new HttpClientHandler();
//            handler.ServerCertificateCustomValidationCallback =
//                (message, cert, chain, errors) =>
//                {
//                    if (cert.Issuer.Equals("CN=localhost"))
//                        return true;
//                    return errors == System.Net.Security.SslPolicyErrors.None;
//                };
//            return handler;
//        }
//    }
//}
