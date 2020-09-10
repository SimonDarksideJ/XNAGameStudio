//-----------------------------------------------------------------------------
// XnaGraphicsProfileChecker.cpp
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "stdafx.h"
#include "MainForm.h"

using namespace XnaGraphicsProfileChecker;


[STAThreadAttribute]
int main(array<System::String ^> ^args)
{
    // Enabling Windows XP visual effects before any controls are created
    Application::EnableVisualStyles();
    Application::SetCompatibleTextRenderingDefault(false); 

    // Create the main window and run it
    Application::Run(gcnew MainForm());
    return 0;
}
