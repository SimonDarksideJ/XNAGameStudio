//-----------------------------------------------------------------------------
// ReportDocument.cpp
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "stdafx.h"
#include "ReportDocument.h"
#include "ProfileChecker.h"

using namespace System::Security;
using namespace XnaGraphicsProfileChecker;


// Generates a report listing what graphics profiles are supported by the current hardware.
ReportDocument::ReportDocument()
{
    report = gcnew ElementList();

    try
    {
        GraphicsAdapter^ adapter = GraphicsAdapter::DefaultAdapter;

        // Output the graphics card name and device identifiers.
        report->Add("h1", adapter->Description);

        ElementList^ adapterDetails = gcnew ElementList();

        adapterDetails->Add("li", String::Format("Vendor ID: {0:X4}", adapter->VendorId));
        adapterDetails->Add("li", String::Format("Device ID: {0:X4}", adapter->DeviceId));
        adapterDetails->Add("li", String::Format("Subsystem: {0:X8}", adapter->SubSystemId));
        adapterDetails->Add("li", String::Format("Revision: {0:X8}", adapter->Revision));

        report->Add("ul", adapterDetails);

        for each (GraphicsProfile profile in Enum::GetValues(GraphicsProfile::typeid))
        {
            report->Add("hr", String::Empty);
            report->Add("h1", profile);

            // Run our profile checking logic.
            ProfileChecker^ profileChecker = gcnew ProfileChecker(profile);

            // As a sanity check, also ask the XNA Framework whether this profile is supported.
            bool xnaResult = adapter->IsProfileSupported(profile);

            // Output the results.
            if (profileChecker->IsSupported)
            {
                report->Add("p", xnaResult ? "Supported" : "Yikes! I think this profile should be supported, but XNA says it is not");
            }
            else
            {
                report->Add("p", xnaResult ? "Not supported" : "Yikes! I think this profile should not be supported, but XNA says it is");

                ElementList^ errorDetails = gcnew ElementList();

                for each (String^ error in profileChecker->Errors)
                {
                    errorDetails->Add("li", error);
                }

                report->Add("ul", errorDetails);
            }
        }
    }
    catch (Exception^ exception)
    {
        report->Add("p", "Yikes! Check failed with this exception:");
        report->Add("pre", exception);
    }
}


// Formats the report as a text string.
String^ ReportDocument::ToText(ElementList^ elements)
{
    String^ result = String::Empty;

    for each (Element^ element in elements)
    {
        ElementList^ nestedList = dynamic_cast<ElementList^>(element->Item2);

        if (nestedList)
            result += ToText(nestedList);
        else
            result += element->Item2 + Environment::NewLine;
    }

    return result;
}


// Formats the report as an HTML document.
String^ ReportDocument::ToHtml(ElementList^ elements)
{
    String^ result = String::Empty;

    for each (Element^ element in elements)
    {
        String^ contents;
        
        ElementList^ nestedList = dynamic_cast<ElementList^>(element->Item2);

        if (nestedList)
            contents = ToHtml(nestedList);
        else
            contents = SecurityElement::Escape(element->Item2->ToString());

        result += String::Format("<{0}>{1}</{0}>\n", element->Item1, contents);
    }

    return result;
}
