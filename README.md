# Introduction 
Requiring consent to multiple AAD applications as part of an O365 solution on-boarding often leads to numerous prompts and a bad user experience. This code story illustrates how to use the AAD Graph API to allow a tenant admin to pre-consent to multiple AAD Applications by consenting to a single booststrap application that performs the rest of the magic necessary.

Note that these are code samples intenting to demonstrating how to perform programmatic consent. You will need to modify them to your needs and adjust the code for your specific use case & set of applications.

# Getting Started
These code samples come in two flavors: a console application which can be distributed as an .exe, or a web app that users can visit to complete consent. Visit the README of each for specific setup instructions:
- [Console](Console/README.md)
- [Web](Web/README.md)

Overall, the process looks like:
1. Setup the AAD applications for your solution(s)
2. Add a new bootstrap AAD application with the permission 'Sign in and read user profile' from both the *Windows Azure Active Directory* and *Microsoft Graph* APIs
3. Copy the bootstrap application information into the `Constants.cs`
4. Configure the `GraphRequests.cs` file per the permissions required by your solution's AAD applications
5. Run it!
6. After the user signs-in with an admin account and consents to the bootstrap app, their tenant will be configured to grant consent for your solution's other applications automatically.

You will find several comments documenting the code, but a lengthier explanation of the programattic consent process can be found in [this blog post](http://blog.codemoggy.com/index.php/2017/08/28/granting-pre-consent-for-multiple-aad-applications/).

# About the authors
These code samples were written by [@codemoggy](https://github.com/codemoggy), [@stewartadam](https://github.com/stewartadam) and [@dkisselev](https://github.com/dkisselev).