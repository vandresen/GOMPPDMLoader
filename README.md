# GOMPPDMLoader

Loading of BOEM (Gulf of Mexico) data into PPDM

The program will automatically download wellbore or deviations surveys from the web for you and load into your PPDM SQL Server database. Only SQL Server is supported.

The release have a self contained executable that you can download. This does not have a certificate so you will get a warning when using it.

Usage: GOMPPDMLoaderConsole [options]

Options:

--connection (REQUIRED) Database connection string

--datatype <Deviations|Wellbore> (REQUIRED) Data type to process: Wellbore or Markerpick

--version Show version information

-?, -h, --help Show help and usage information
