# Auto Translate for Umbraco

A free open source package for Umbraco CMS which uses Azure Text Translation 
to automatically translate content and dictionary items.

### Dev Instructions

Once installed, get a subscription key for the Azure Text Translation Service
and put it in your app settings with the api url like this:

<add key="AzureTranslateApiUrl" value="https://api.cognitive.microsofttranslator.com/translate?api-version=3.0"/>
<add key="AzureTranslateSubscriptionKey" value="your-subscription-key-here"/>

### DISCLAIMER

This package is only as good as the translations you get from the
Azure Text Translation Service. The translations should be checked before you
publish them to the world.

------------------------------------------------------------------------------

### Turning coffee into code

This package is made by Paul Seal from codeshare.co.uk

It is a side project that I work on in the evenings. If you would like to help
me to write more code in the evenings to work on this package or other ones 
then please consider buying me a coffee using the link below:

[https://codeshare.co.uk/coffee](https://codeshare.co.uk/coffee)
