using log4net;
using log4net.Core;
using log4net.Layout;
using log4net.Layout.Pattern;
using log4net.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace iSchool.Infrastructure.Logs
{
    public class CustomPatternLayout : PatternLayout
    {
        public CustomPatternLayout()
        {
            AddConverter("cst", typeof(CustomPatternLayoutConverter));
        }
    }

    public class CustomPatternLayoutConverter : PatternLayoutConverter
    {
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            //var logEntry = loggingEvent.MessageObject as LogMessage;

            if (Option == null)
                WriteDictionary(writer, loggingEvent.Repository, loggingEvent.GetProperties());
            else
                WriteObject(writer, loggingEvent.Repository, LookupProperty(Option, loggingEvent));

        }

        object LookupProperty(string property, LoggingEvent loggingEvent)
        {
            object propertyValue = string.Empty;
            var propertyInfo = loggingEvent.MessageObject.GetType().GetProperty(property);
            if (propertyInfo != null)
                propertyValue = propertyInfo.GetValue(loggingEvent.MessageObject, null);
            return propertyValue;
        }
    }
}
