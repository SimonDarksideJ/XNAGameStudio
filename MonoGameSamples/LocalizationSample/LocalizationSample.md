
#Localization Sample

This sample shows how to localize an XNA Framework game into multiple languages.

##Sample Overview

This sample demonstrates three aspects of game localization:

* Loading text strings from .resx resource files, and providing different versions for each language.
* Building fonts that include the Unicode characters needed to display different languages, including East Asian languages such as Japanese and Korean.
* Localizing other game assets such as textures, models, and sound effects.

The sample is localized into Danish, French, Japanese, and Korean, and also includes a default English configuration (can you guess what countries people on the XNA team come from? :-). It chooses what language to display based on your current system settings.

##To change the language setting on Windows

1. In Control Panel, click Clock, Language, and Region.
2. In the Region and Language dialog box, click the Formats tab, and then click the Format box and select the lanuage.
3. Click OK.

##To change the language setting on Xbox 360

1. Press the Guide button. The system UI is displayed.
2. Scroll right through the blades and select System Settings, and then Console Settings.
3. Change the language option, and then restart the sample to ensure the new language is in effect.

#Sample Controls

This sample uses the following keyboard and gamepad controls.

Action | Keyboard control |Gamepad control
|---|---|---|
Exit | ESC or ALT+F4 | BACK

#How the Sample Works

##Translating Text

The first step in translating your game text is to make sure all your strings are defined in a resource file (.resx). To create a resource file, right-click on your project in Solution Explorer, choose Add, and then choose New Item, and then select the Resources File template. You can call this anything you like—the one in this sample is named Strings.resx. Double-clicking the .resx file opens a designer where you can enter your game strings, giving each one a name and then entering its text value. If you need to include line breaks within a string, press SHIFT+ENTER.

You will notice that when you added the Strings.resx file, Visual Studio also created a Strings.Designer.cs file, which is normally collapsed under the main .resx entry (click the plus sign (+) next to your .resx file to show it). This file contains a generated C# wrapper class that makes it easy to access your text strings. In your game code, you can now just look up Strings.NameOfString (or if you chose some other name than Strings.resx, this would be MyResxName.NameOfString ), instead of hard coding the actual string constant.

Once all your text is defined in the .resx file, and your game code is accessing it through the generated wrapper class, it is trivial to add translations for different languages. You just add a second .resx file, using a simple naming convention. If the default resource file was called Strings.resx, a French translation would be called Strings.fr.resx, and a Japanese one would be Strings.ja.resx. You can also add translations for specific countries as well as languages. A file called Strings.en-GB.resx, for example, would be used for English in Great Britain, but not the United States (see the [Language and Country Codes](file:///C:/Development/GitHub/XNAGameStudio/MonoGameSamples/LocalizationSample/Localization.htm#lang_country_codes) section for more detail on this).

If you add these secondary resource files correctly, you will only create a .resx file. There should not be a plus sign (+) next to this because these files should not generate a C# wrapper class. You must be careful when adding these secondary resources. Type the full name in the Add dialog box, including the .resx extension. It will not work if you leave that off. If you add a file with the wrong name, delete it and add a new one because renaming an existing resource may not hook up everything correctly.

Once your secondary resources are in place, you can add translated strings to them. It is not necessary to include all your strings in every resource. If you leave some out, those will be taken from the default resource file instead. You are free to translate just the things that you want to translate, while leaving other strings in your original language.

The final step is to tell the resource manager what language to use. This is done by setting *Strings.Culture = CultureInfo.CurrentCulture* in your game constructor.

##Unicode Fonts

Once you have text in more than one language, you will probably notice that some languages use more characters than are included by the default .spritefont template. By default, trying to draw these characters will throw an exception. If you set the <DefaultCharacter> element in your .spritefont XML, that will be automatically substituted in place of any missing characters, but just replacing all your Japanese text with question marks or spaces probably isn't the best long-term solution!

You can control what characters are included in the font by altering the *<CharacterRegions>* section of the *.spritefont XML*. You can add as many character regions as you like to include different portions of the [Unicode character](http://unicode.org/charts/) set lists where each character can be found. This is a good solution for languages such as French or German that require only a few accented letters in addition to the standard ASCII character range.

For other languages, however, especially East Asian ones such as Japanese, manually specifying character regions is not such a good solution. The Japanese character set is huge, and trying to include it all would produce a ridiculously large font. Fortunately, however, most games use only a small fraction of the available characters, so we can optimize our fonts by bothering to include only those characters we really need.

This process is implemented by the build time classes in the *LocalizationPipeline* project. If you look at the *Font.spritefont* file used in this sample, you will notice that the Asset Type in the XML header has been changed from the normal *SpriteFontDescription* class to specify a custom *LocalizationPipeline.LocalizedFontDescription* instead. This type inherits from SpriteFontDescription, and adds a new property to specify what resource files contain our game text. The processor setting for *Font.spritefont* has been changed from the standard *FontDescriptionProcessor* to instead use the custom *LocalizedFontProcessor*, which reads all these *.resx* files, scans over them to find what characters they contain, and automatically adds the necessary characters to the font.

To see this in action, set your system to Japanese, then remove the *Strings.ja.resx* reference from the *Font.spritefont* file. The Japanese characters will no longer be included in the font, so when you run the sample, you will now see question marks instead of Japanese text.

##Localizing Other Assets

Text strings and fonts are by far the most important aspect of localizing a game, but you may occasionally want to localize other assets such as textures, models, or sound effects.

The LoadLocalizedAsset function in this sample implements a simple naming convention that can be used to localize any kind of XNB data. The sample contains a texture called Flag.png, plus a number of specialized versions for specific cultures (Flag.fr.png for France, Flag.en-GB.png for Great Britain, and so on). Instead of just calling:

```
        currentFlag = Content.Load<Texture2D>("Flag")
```      

Instead the sample does this:

```
        currentFlag = LoadLocalizedAsset<Texture2D>("Flag")
```      

This will check for a suitable localized version of the asset exists, loading the appropriate version for the current culture, or falling back to the default version of Flag.png if it cannot find a localized one.

##Language and Country Codes

When creating localized .resx files, or different versions of an asset for use with the GetLocalizedAssetName function, there are two ways you can specify the language and/or country.

Most often, you will just use a two-letter language code from [ISO 639-1](http://www.loc.gov/standards/iso639-2/php/English_list.php). The sample uses this convention for the Strings.fr.resx file or the Flag.ja.png file.

Other times you may wish to differentiate by country as well as language. To accomplish this, you can add a two-letter country code from [ISO 3166](http://www.iso.org/iso/english_country_names_and_code_elements) after the language code. The sample uses this convention for the Flag.en-US.png and Flag.en-GB.png files. Both flags are for English-speaking countries, but this specialization allows us to differentiate between the United States and United Kingdom. If you set your system to some other English-speaking country—for example, New Zealand—neither of these flags will match, so the default flag will be displayed instead. Because the sample does not specify any particular French-speaking country for its French flag texture, that will be displayed even if you set your system to French (Canada) or French (Belgium).

##The WPF Font processor
Need text here :D