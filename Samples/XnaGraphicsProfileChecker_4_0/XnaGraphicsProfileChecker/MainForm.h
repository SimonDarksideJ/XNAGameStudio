//-----------------------------------------------------------------------------
// MainForm.h
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#pragma once

#include "ReportDocument.h"

namespace XnaGraphicsProfileChecker
{
    using namespace System::Windows::Forms;


    public ref class MainForm : public System::Windows::Forms::Form
    {
    public:
        MainForm()
        {
            InitializeComponent();

            report = gcnew ReportDocument();

            webBrowser->DocumentText = report->ToHtml();
        }

    protected:
        ~MainForm()
        {
            if (components)
            {
                delete components;
            }
        }


    private: 
        ReportDocument^ report;

        System::Windows::Forms::WebBrowser^ webBrowser;
        System::Windows::Forms::Button^ copyToClipboard;


        void CopyToClipboard_Click(System::Object^ sender, System::EventArgs^ e)
        {
            Clipboard::SetData(DataFormats::Text, report->ToText());
        }


        /// <summary>
        /// Required designer variable.
        /// </summary>
        System::ComponentModel::Container ^components;

#pragma region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        void InitializeComponent(void)
        {
            this->webBrowser = (gcnew System::Windows::Forms::WebBrowser());
            this->copyToClipboard = (gcnew System::Windows::Forms::Button());
            this->SuspendLayout();
            // 
            // webBrowser
            // 
            this->webBrowser->AllowNavigation = false;
            this->webBrowser->AllowWebBrowserDrop = false;
            this->webBrowser->Dock = System::Windows::Forms::DockStyle::Fill;
            this->webBrowser->Location = System::Drawing::Point(0, 0);
            this->webBrowser->MinimumSize = System::Drawing::Size(20, 20);
            this->webBrowser->Name = L"webBrowser";
            this->webBrowser->Size = System::Drawing::Size(784, 562);
            this->webBrowser->TabIndex = 0;
            this->webBrowser->WebBrowserShortcutsEnabled = false;
            // 
            // copyToClipboard
            // 
            this->copyToClipboard->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Right));
            this->copyToClipboard->Location = System::Drawing::Point(638, 12);
            this->copyToClipboard->Name = L"copyToClipboard";
            this->copyToClipboard->Size = System::Drawing::Size(120, 23);
            this->copyToClipboard->TabIndex = 1;
            this->copyToClipboard->Text = L"CopyTo Clipboard";
            this->copyToClipboard->UseVisualStyleBackColor = true;
            this->copyToClipboard->Click += gcnew System::EventHandler(this, &MainForm::CopyToClipboard_Click);
            // 
            // MainForm
            // 
            this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
            this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
            this->ClientSize = System::Drawing::Size(784, 562);
            this->Controls->Add(this->copyToClipboard);
            this->Controls->Add(this->webBrowser);
            this->Name = L"MainForm";
            this->Text = L"XNA Graphics Profile Checker";
            this->ResumeLayout(false);

        }
#pragma endregion
    };
}

