# TimeTravelersSheetBridge
The tool to inject all translations from our Google Spreadsheet to .cfg.bin and .pck files.

## Configuration

The tool needs to load data from the Google Spreadsheet.

Configurations are to be done in `config.json` and via `command line arguments`, explained below:

### config.json

|Category|Key|Description|
|--|--|--|
|`Logic.Business.TimeTravelersManagement`|`PatchMapPath`|The file path, relative to TimeTravelersSheetBridge, that contains a mapping of font characters. The same mapping used in [FontPatcher](https://github.com/Time-Travelers-Translation/FontPatcher).|
|`Logic.Business.TranslationManagement`|`TranslationSheetId`|The ID of the Google Sheet to request translations from. Read [here](https://stackoverflow.com/questions/36061433/how-do-i-locate-a-google-spreadsheet-id) to learn how to retrieve the ID from your Google Sheet.|
|`Logic.Domain.GoogleSheetsManagement.OAuth2`|`ClientId`|The Client ID of an OAuth2 authentication pair to request data from Google Sheets via the API. Read [here](https://developers.google.com/identity/protocols/oauth2) to learn about the Google API and creating OAuth2 credentials.|
|`Logic.Domain.GoogleSheetsManagement.OAuth2`|`ClientSecret`|The Client Secret of an OAuth2 authentication pair to request data from Google Sheets via the API. Read [here](https://developers.google.com/identity/protocols/oauth2) to learn about the Google API and creating OAuth2 credentials.|

### Command Line

|Command|Description|
|--|--|
|-h|Shows a help text on how to use this tool on the command line and lists all the commands from this table as well.|
|-i|The absolute folder path containing files to inject text into.|
|-m|The mode of injection, so the tool knows which texts to retrieve and in which files in the folder from -i to inject them into.|
|-a|Additional arguments for -m. Is only used for mode `scn` to retrieve a patched .xf font for alignment of Hint texts.|

## Injection Modes

The mode of injection defines from which Google Spreadheet tables the texts are taken and in which files they are injected.

|Mode|Table Name|Files|
|--|--|--|
|`tuto`|`Tutorial`|`Tuto_List_ja.cfg.bin`, `TUTOxxx_ja.cfg.bin`|
|`tips`|`TIPS`|`Tip_List_ja.cfg.bin`, `TIPxxx_ja.cfg.bin`|
|`help`|`Help`|`Help_List_ja.cfg.bin`, `HELPxxx_ja.cfg.bin`|
|`outline`|`Outlines`|`OUTLINE_x_ja.cfg.bin`|
|`staffroll`|`Staffroll`|`staffroll_ja.cfg.bin`|
|`flow`|`Titles`|`tt1.flo`|
|`scn`|`Titles`, `Hints`, `Decisions`|`tt1.scn`, `tt1_A01A_0000.scn`, `tt1_P01A_0000.scn`, `tt1_I02A_0000.scn`, `tt1_I03A_0000.scn`, `tt1_I04A_0000.scn`, `tt1_C05A_0020.scn`, `tt1_I06A_0000.scn`, `tt1_M07A_0000.scn`|
|`c1`|`1`, `Names`|`A01.pck`, `C01.pck`, `I01.pck`, `P01.pck`|
|`c2`|`2`, `Names`|`C02.pck`, `H02.pck`, `I02.pck`, `P02.pck`, `R02.pck`, `S02.pck`|
|`c3`|`3`, `Names`|`C03.pck`, `H03.pck`, `I03.pck`, `P03.pck`, `R03.pck`, `S03.pck`|
|`c4`|`4`, `Names`|`C04.pck`, `H04.pck`, `I04.pck`, `P04.pck`, `R04.pck`, `S04.pck`|
|`c5`|`5`, `Names`|`C05.pck`, `H05.pck`, `I05.pck`, `P05.pck`, `R05.pck`, `S05.pck`|
|`c6`|`6`, `Names`|`C06.pck`, `H06.pck`, `I06.pck`, `P06.pck`, `R06.pck`, `S06.pck`|
|`c7`|`7`, `Names`|`M07.pck`|
