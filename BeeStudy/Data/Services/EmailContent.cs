using BeeStudy.Models;
using Microsoft.AspNetCore.Hosting;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BeeStudy.Data.Services
{
    public class EmailContent
    {

        private IHostingEnvironment _environment;


        public EmailContent(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public string BuildEmailBody(Learner learner, Course course)
        {
            var webRoot = _environment.WebRootPath; //get wwwroot Folder

            //Get TemplateFile located at wwwroot/Templates/EmailTemplate/Register_EmailTemplate.html  
            var pathToFile = _environment.WebRootPath
                    + Path.DirectorySeparatorChar.ToString()
                    + "Templates"
                    + Path.DirectorySeparatorChar.ToString()
                    + "ConfirmationEmailTemplate.html";

            var builder = new BodyBuilder();

            //Read Content of Template file using StreamReader and append to BodyBuilder()
            using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
            {
                builder.HtmlBody = SourceReader.ReadToEnd();
            }

            string messageBody = string.Format(builder.HtmlBody,
                        learner.Name,
                        learner.Email,
                        course.Title,
                        course.Headline,
                        course.ImageUrl,                        
                        course.ListPrice,
                        course.CurrentPrice,
                        course.Browser
                        );

            return messageBody;
        }


    }
}
