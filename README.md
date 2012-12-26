Elastacloud.WindowsAzure.ActiveDirectory
========================================

A library which consumes the Windows Azure Active Directory Graph API and makes it easier to build and use principals from a web tier.

This library has dependencies on the the Windows Azure AAL library which is downloable from Github and also the OData, EDM and Spatial libraries.

There are two projects. The library itself and the test project which shows how to use it. 

In order to configure the test project add the following details to the app.config.

&lt;appSettings&gt;
    &lt;add key="DomainName" value="example.onmicrosoft.com" /&gt;
    &lt;add key="AppPrincipalId" value="ffffffff-6c33-4b55-8aa4-ffffffffff" /&gt;
    &lt;add key="SymmetricKey" value="FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF=" /&gt;
&lt;/appSettings>
