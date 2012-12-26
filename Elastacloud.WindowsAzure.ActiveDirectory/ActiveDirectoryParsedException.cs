using System;
using System.IO;
using System.Xml.Serialization;

namespace Elastacloud.WindowsAzure.ActiveDirectory
{
    /// <summary>
    /// Exception type which represents the DataServiceException thrown by the ADO.NET Data Service
    /// </summary>
    [Serializable]
    public class ActiveDirectoryParsedException : XmlErrorResponse
    {
        /// <summary>
        /// Xml serializer
        /// </summary>
        private static readonly XmlSerializer XmlSerializer = new XmlSerializer(typeof(XmlErrorResponse));

        /// <summary>
        /// Initializes a new instance of the <see>
        ///                                       <cref>ParsedException</cref>
        ///                                   </see>
        ///     class.
        /// </summary>
        public ActiveDirectoryParsedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see>
        ///                                       <cref>ParsedException</cref>
        ///                                   </see>
        ///     class.
        /// </summary>
        /// <param name="code">Error code.</param>
        /// <param name="message">Error message.</param>
        public ActiveDirectoryParsedException(string code, string message)
        {
            this.Code = code;
            this.Message = new ErrorMessage() { Value = message };
        }

        /// <summary>
        /// Initializes a new instance of the <see>
        ///                                       <cref>ParsedException</cref>
        ///                                   </see>
        ///     class.
        /// </summary>
        /// <param name="error">Error object.</param>
        public ActiveDirectoryParsedException(XmlErrorResponse error)
        {
            this.Code = error.Code;
            this.InnerError = error.InnerError;
            this.Message = error.Message;
            this.Values = error.Values;
        }

        /// <summary>
        /// Parse the given exception.
        /// </summary>
        /// <param name="exc">Encountered exception.</param>
        /// <returns>Parsed exception.</returns>
        /// <exception cref="ArgumentException">Exception is not an InvalidOperationException</exception>
        /// <exception cref="ArgumentNullException">Excepion is null</exception>
        public static ActiveDirectoryParsedException Parse(Exception exc)
        {
            if (exc == null)
            {
                throw new ArgumentNullException("exc is null");
            }

            if (!(exc is InvalidOperationException))
            {
                throw new ArgumentException("exc is not of type InvalidOperationException");
            }

            var parsedException = new ActiveDirectoryParsedException();

            string messageToBeParsed = exc.Message;
            if (exc.InnerException != null)
            {
                messageToBeParsed = exc.InnerException.Message;
            }

            try
            {
                var stringReader = new StringReader(messageToBeParsed);
                var errorObject = XmlSerializer.Deserialize(stringReader) as XmlErrorResponse;
                parsedException = new ActiveDirectoryParsedException(errorObject);
            }
            catch (InvalidOperationException)
            {
            }

            if (String.IsNullOrEmpty(parsedException.Code))
            {
                parsedException.Code = messageToBeParsed;
            }

            return parsedException;
        }
    }

#region Helper Classes

     [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")]
    [System.Xml.Serialization.XmlRootAttribute("error", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", IsNullable = false)]
    public partial class XmlErrorResponse
    {
        private string codeField;

        private ErrorMessage messageField;

        private ErrorInnererror innererrorField;

        private ErrorValues valuesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("code", IsNullable = true)]
        public string Code
        {
            get
            {
                return this.codeField;
            }
            set
            {
                this.codeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("message", IsNullable = true)]
        public ErrorMessage Message
        {
            get
            {
                return this.messageField;
            }
            set
            {
                this.messageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("innererror")]
        public ErrorInnererror InnerError
        {
            get
            {
                return this.innererrorField;
            }
            set
            {
                this.innererrorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("values")]
        public ErrorValues Values
        {
            get
            {
                return this.valuesField;
            }
            set
            {
                this.valuesField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")]
    public partial class ErrorMessage
    {

        private string langField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("lang", Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string Lang
        {
            get
            {
                return this.langField;
            }
            set
            {
                this.langField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")]
    public partial class ErrorInnererror
    {

        private string messageField;

        private string typeField;

        private string stacktraceField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("message")]
        public string Message
        {
            get
            {
                return this.messageField;
            }
            set
            {
                this.messageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("type")]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("stacktrace")]
        public string StackTrace
        {
            get
            {
                return this.stacktraceField;
            }
            set
            {
                this.stacktraceField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")]
    public partial class ErrorValues
    {

        private ErrorValuesErrorDetail[] errorDetailField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("detail", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ErrorValuesErrorDetail[] ErrorDetail
        {
            get
            {
                return this.errorDetailField;
            }
            set
            {
                this.errorDetailField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")]
    public partial class ErrorValuesErrorDetail
    {

        private string nameField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("name", Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("value", Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", IsNullable = false)]
    public partial class NewDataSet
    {

        private XmlErrorResponse[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("items")]
        public XmlErrorResponse[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }
    }

#endregion
}
