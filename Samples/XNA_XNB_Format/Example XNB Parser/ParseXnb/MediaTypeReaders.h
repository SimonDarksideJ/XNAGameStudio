#pragma once

#include "TypeReader.h"


class SoundEffectReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Audio.SoundEffect"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.SoundEffectReader"; }
    
    virtual void Read(ContentReader* reader);
};


class SongReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Media.Song"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.SongReader"; }
    
    virtual void Read(ContentReader* reader);
};


class VideoReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Media.Video"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.VideoReader"; }
    
    virtual void Read(ContentReader* reader);
};
