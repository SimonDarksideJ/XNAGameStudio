//-----------------------------------------------------------------------------
// ReportDocument.h
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#pragma once

namespace XnaGraphicsProfileChecker
{
    using namespace System;
    using namespace System::Collections::Generic;


    // Formats profile checker results as an HTML or text report.
    ref class ReportDocument
    {
    public:
        ReportDocument();

        String^ ToText() { return ToText(report); }
        String^ ToHtml() { return ToHtml(report); }


    private:
        // Each line of the report is stored as a tuple, where the first item
        // is an HTML formatting element (p, h1, etc), and the second is either
        // a text string or a nested ElementList (for recursive elements).
        typedef Tuple<String^, Object^> Element;


        // Helper method provides a nicer Add syntax.
        ref class ElementList : List<Element^>
        {
        public:
            void Add(String^ element, Object^ contents)
            {
                Add(gcnew Element(element, contents));
            }
        };


        // The entire report is stored as a list of elements.
        ElementList^ report;


        // Recursive formatting helper methods.
        static String^ ToText(ElementList^ elements);
        static String^ ToHtml(ElementList^ elements);
    };
}
