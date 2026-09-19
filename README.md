# Payment Management (VA009)

**Payment schedules and payment runs for Onfinity ERP.** An add-on module for [Onfinity ERP and CRM](https://github.com/VIENNA-Advantage-ERP-CRM/Official-VAStandard-ERP-CRM) (formerly VIENNA Advantage), the open source ERP in C#/.NET.

Generates the payment schedule of an invoice or order from its payment terms, and gives one screen for the work around payments: splitting a schedule into several instalments, paying against AR and AP invoices, advance orders, credit notes and debit notes, payment batches, payment methods and classes, bank mandates, and cheque printing parameters.

Windows and forms: Payment Schedule and Order Payment Schedule; Payment Schedule Batch; Payment Method and Payment Class; Payment Mandate; Check Printing Parameters.

## Installing

Modules are packaged with their Application Dictionary entries, code and files, and installed through the Onfinity Market inside the system or from the Module Management window (System Administration role). The module version must match the Onfinity release you run. Packaged releases of the system are on [SourceForge](https://sourceforge.net/projects/erp-crm-advant/files/); the Market is described in the [documentation](https://viennaadvantage.atlassian.net/wiki/spaces/VA/pages/3670266/About+VIENNA+Advantage+Market).

## Repository layout

| Folder | Contents |
|---|---|
| `ViennaAdvantage` | The module's model classes and processes. |
| `ViennaAdvantageSvc` | Its services. |
| `ViennaAdvantageWeb` | Its area: windows, forms, scripts. |

Build with `ViennaAdvantageWeb.sln` (Visual Studio 2019 or later, .NET Framework) against the [framework](https://github.com/VIENNA-Advantage-ERP-CRM/Official-VAFramework) and [base libraries](https://github.com/VIENNA-Advantage-ERP-CRM/Official-VABaseFiles) of the matching release; see the ERP repository's [BUILD.md](https://github.com/VIENNA-Advantage-ERP-CRM/Official-VAStandard-ERP-CRM/blob/master/BUILD.md).

| | |
|---|---|
| Website | https://onfinity.io |
| Community portal | https://login.onfinity.io/register.aspx |

## Licence

Eclipse Public Licence. See https://onfinity.io/open-source-erp.php.
