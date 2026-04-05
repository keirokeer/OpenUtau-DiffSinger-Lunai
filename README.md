# Fork说明

本Fork在[keirokeer/OpenUtau-DiffSinger-Lunai
](https://github.com/keirokeer/OpenUtau-DiffSinger-Lunai)基础上增加了歌词快捷编辑功能，同时优化了歌词编辑窗口，减少歌词编辑时的卡顿，详见下方提示词。

# Vibe Code 说明
使用GPT5.4-Codex进行Vibe Code。

Prompt:
(1)
1、点击音符，把该音符的歌词换为“+~”，原来此位置的歌词不要去掉而是所有音符往后顺延；
2、按住某个键（可以定为Alt，如果被占用你换一个），点击音符某处，使用分割工具在此处将此音符拆开，与分割工具不同的是拆出来的第二个音不要变为“+”而是后面的歌词向前顺延补进来；
3、在拖动音符边界时，如果是音和音的边界，自动编辑相邻两个音的长度，即把Alt+拖动的行为和直接拖动的行为对调。
该模式的开关放在显示提示按钮的旁边。
(2)
编辑歌词面板中编辑/插入/删除歌词会卡几秒中，试优化。
(3)
使得渲染时进行重校验/重算，实现编辑不卡顿，开始渲染再卡一下（被视为可接受的），同时点击应用/取消仍保持原有逻辑，即取消可回退。另外，在Windows下（其他平台未测试不知晓），编辑歌词窗口强制最前，下方的界面无法操作，去掉这一限制。评估这一需求在全局和具体的的合理性，若合理，开始修改。
(4)
审阅Commit：这两个更改，以最小更改原则、代码可读性原则以及全局影响原则评价代码质量，我们是在别人的仓库上fork并修改，对未来pull(rebase)主仓库代码更新是否阻碍较小？就这几点进行最小改动版修正优化。

# 编译指南

$ver=(git describe --tags --match '[0-9]*' --abbrev=0).Trim(); $sha=(git rev-parse --short HEAD).Trim(); $env:APPVEYOR_BUILD_VERSION=$ver; dotnet publish OpenUtau/OpenUtau.csproj -c Release -r win-x64 --self-contained true -o bin/win-x64 /p:Version=$ver /p:FileVersion=$ver /p:AssemblyVersion=$ver /p:InformationalVersion="$ver+$sha"u-DiffSinger-Lunai\OpenUtau.Plugin.Builtin\SyllableBasedPhonemizer.cs


# OpenUtau

OpenUtau is a free, open-source editor made for the UTAU community.

[![Build](https://img.shields.io/github/actions/workflow/status/stakira/OpenUtau/build.yml?style=for-the-badge)](https://github.com/stakira/OpenUtau/actions/workflows/build.yml)
[![Discord](https://img.shields.io/discord/551606189386104834?style=for-the-badge&label=discord&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/UfpMnqMmEM)
[![QQ Qroup](https://img.shields.io/badge/QQ-485658015-blue?style=for-the-badge)](https://qm.qq.com/cgi-bin/qm/qr?k=8EtEpehB1a-nfTNAnngTVqX3o9xoIxmT&jump_from=webapi)
[![Trello](https://img.shields.io/badge/trello-go-blue?style=for-the-badge&logo=trello)](https://trello.com/b/93ANoCIV/openutau)

## Getting started

[![Download](https://img.shields.io/static/v1?style=for-the-badge&logo=github&label=download&message=windows-x64&labelColor=FF347C&color=4ea6ea)](https://github.com/stakira/OpenUtau/releases/latest/download/OpenUtau-win-x64.zip)</br>
[![Download](https://img.shields.io/static/v1?style=for-the-badge&logo=github&label=download&message=windows-x86&labelColor=FF347C&color=4ea6ea)](https://github.com/stakira/OpenUtau/releases/latest/download/OpenUtau-win-x86.zip)</br>
[![Download](https://img.shields.io/static/v1?style=for-the-badge&logo=github&label=download&message=macos-x64&labelColor=FF347C&color=4ea6ea)](https://github.com/stakira/OpenUtau/releases/latest/download/OpenUtau-osx-x64.dmg)</br>
[![Download](https://img.shields.io/static/v1?style=for-the-badge&logo=github&label=download&message=linux-x64&labelColor=FF347C&color=4ea6ea)](https://github.com/stakira/OpenUtau/releases/latest/download/OpenUtau-linux-x64.tar.gz)

It is **strongly recommended** that you read these Github wiki pages before using the software.
- [Getting-Started](https://github.com/stakira/OpenUtau/wiki/Getting-Started)
- [Resamplers](https://github.com/stakira/OpenUtau/wiki/Resamplers-and-Wavtools)
- [Phonemizers](https://github.com/stakira/OpenUtau/wiki/Phonemizers)
- [FAQ](https://github.com/stakira/OpenUtau/wiki/FAQ)

- [中文使用说明](https://opensynth.miraheze.org/wiki/OpenUTAU/%E4%BD%BF%E7%94%A8%E6%96%B9%E6%B3%95)

## How to contribute

Tried OpenUtau and not satisfied? Don't just walk away! You can help:
- Report issues on our [Discord server](https://discord.gg/UfpMnqMmEM) or Github.
- Suggest features on Discord or Github.
- Add or update translations for your language on [Crowdin](https://crowdin.com/project/oxygen-dioxideopenutau).

Know how to code? Got an idea for an improvement? Don't keep it to yourself!
- Contribute fixes via pull requests.
- Check out the development roadmap on [Trello](https://trello.com/b/93ANoCIV/openutau) and discuss it on Discord.

## Plugin development

Want to contribute plugins to help other users? Check out our API documentation:
- [Editing Macros API Document](OpenUtau.Core/Editing/README.md)
- [Phonemizers API Document](OpenUtau.Core/Api/README.md)

## Main features

Navigate the interface naturally and fluently using the mouse and scroll wheel. Keyboard shortcuts are also available.

![Editor](Misc/GIFs/editor.gif)

Easily create songs and covers using the feature-rich MIDI editor.

![Editor](Misc/GIFs/editor2.gif)

Create expressive vibratos with the easy-to-use vibrato editor.

![Vibrato](Misc/GIFs/vibrato.gif)

Pre-rendering and built-in resamplers let you quickly preview your work.

![Playback](Misc/GIFs/playback.gif)

See the [Getting-Started Wiki page](https://github.com/stakira/OpenUtau/wiki/Getting-Started) for more!

## All features
- Modern user experience.
- Easy navigation using the mouse and keyboard.
- Feature-rich MIDI editor.
  - Support for importing VSQX (Vocaloid 4) tracks.
- Selective backward compatibility with UTAU.
  - OpenUtau aims to solve problems with fewer steps. It is not designed to replicate UTAU features exactly.
- Extensible real-time phonetic editing.
  - Includes phonemizers for different phonetic systems (VCV, CVVC, Arpasing, etc.) in many different languages (English, Japanese, Chinese, Korean, Russian and more).
- Expressions replace the standard UTAU "flags" for tuning.
  - The built-in WORLDLINE-R resampler supports curve tuning, similar to many vocal synth editors.
- Internationalisation, including UI translation and file system encoding support.
  - Unlike UTAU, there is no need to change your system locale to use OpenUtau.
- Smooth preview/rendering experience.
  - Pre-rendering allows OpenUtau to render vocals before playback, saving time during editing and tuning.
- Supports ENUNU AI singers. See the [ENUNU wiki page](https://github.com/stakira/OpenUtau/wiki/ENUNU-NNSVS-Support) for more info.
- Easy-to-use plugin system.
- Versatile resampling engine interface.
  - Compatible with most UTAU resamplers.
- Runs on Windows (32/64 bit), macOS, and Linux.

### What it doesn't do
- While OpenUtau can do very minimal mixing, it will not replace your digital audio workstation of choice.
- OpenUtau does not aim for Vocaloid compatibility, except for some limited features.
