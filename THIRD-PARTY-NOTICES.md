# Third-Party Notices

**[Polski](#polski) | [English](#english)**

---

# Polski

## Cel dokumentu

Dokument zawiera wykaz pakietów zewnętrznych widocznych w konfiguracji projektu.

Obecność pakietu w `Packages/manifest.json` lub `Packages/packages-lock.json` nie oznacza automatycznie, że jego kod lub zasoby są wykorzystywane albo redystrybuowane przez projekt.

## Pakiety Unity

Poniższe wersje pochodzą z przekazanego pliku `packages-lock.json`.

| Pakiet                        | Wersja | Opis                       |
| ----------------------------- | -----: | -------------------------- |
| `com.unity.2d.animation`      |  9.2.0 | 2D Animation               |
| `com.unity.2d.aseprite`       |  1.1.8 | Aseprite Importer          |
| `com.unity.2d.common`         |  8.1.0 | Wspólne komponenty 2D      |
| `com.unity.2d.pixel-perfect`  |  5.1.0 | Pixel Perfect              |
| `com.unity.2d.psdimporter`    |  8.1.0 | PSD Importer               |
| `com.unity.2d.sprite`         |  1.0.0 | Sprite                     |
| `com.unity.2d.spriteshape`    |  9.1.0 | SpriteShape                |
| `com.unity.2d.tilemap`        |  1.0.0 | Tilemap                    |
| `com.unity.2d.tilemap.extras` |  3.1.3 | Tilemap Extras             |
| `com.unity.burst`             | 1.8.19 | Burst                      |
| `com.unity.collab-proxy`      | 2.12.4 | Integracja kontroli wersji |
| `com.unity.collections`       |  1.2.4 | Collections                |
| `com.unity.ext.nunit`         |  1.0.6 | NUnit                      |
| `com.unity.feature.2d`        |  2.0.1 | 2D Feature Set             |
| `com.unity.ide.rider`         | 3.0.35 | Rider Integration          |
| `com.unity.ide.visualstudio`  | 2.0.22 | Visual Studio Integration  |
| `com.unity.inputsystem`       | 1.14.2 | Input System               |
| `com.unity.mathematics`       |  1.2.6 | Mathematics                |
| `com.unity.test-framework`    | 1.1.33 | Test Framework             |
| `com.unity.textmeshpro`       |  3.0.7 | TextMeshPro                |
| `com.unity.timeline`          |  1.7.7 | Timeline                   |
| `com.unity.ugui`              |  1.0.0 | Unity UI                   |
| `com.unity.visualscripting`   |  1.9.4 | Visual Scripting           |

## Moduły wbudowane Unity

Projekt zawiera również wpisy `com.unity.modules.*`, które reprezentują moduły wbudowane Unity.

Ich wykorzystanie podlega odpowiednim warunkom licencyjnym Unity.

## Weryfikacja licencji

Przed publikacją należy:

1. sprawdzić dokładną wersję każdego faktycznie wykorzystywanego pakietu;
2. odczytać jego plik `LICENSE`, `LICENSE.md` lub `Third Party Notices.md`;
3. sprawdzić, czy pliki pakietu zostały skopiowane do repozytorium;
4. zachować wymagane informacje copyright i licencyjne;
5. nie przypisywać licencji MIT plikom należącym do innych autorów.

Pakiety mogą być objęte różnymi warunkami. Nie należy zakładać, że wszystkie pakiety Unity mają identyczną licencję.

## Oficjalne dokumenty

- [Unity Companion License](https://unity.com/legal/licenses/unity-companion-license)
- [Unity Package Distribution License](https://unity.com/legal/licenses/unity-package-distribution-license)
- [Unity Legal Documentation](https://unity.com/legal)

---

# English

## Purpose

This document lists third-party packages visible in the project's configuration.

A package appearing in `Packages/manifest.json` or `Packages/packages-lock.json` does not automatically mean that its code or assets are used or redistributed by this project.

## Unity Packages

The versions below are taken from the supplied `packages-lock.json`.

| Package                       | Version | Description                 |
| ----------------------------- | ------: | --------------------------- |
| `com.unity.2d.animation`      |   9.2.0 | 2D Animation                |
| `com.unity.2d.aseprite`       |   1.1.8 | Aseprite Importer           |
| `com.unity.2d.common`         |   8.1.0 | Shared 2D components        |
| `com.unity.2d.pixel-perfect`  |   5.1.0 | Pixel Perfect               |
| `com.unity.2d.psdimporter`    |   8.1.0 | PSD Importer                |
| `com.unity.2d.sprite`         |   1.0.0 | Sprite                      |
| `com.unity.2d.spriteshape`    |   9.1.0 | SpriteShape                 |
| `com.unity.2d.tilemap`        |   1.0.0 | Tilemap                     |
| `com.unity.2d.tilemap.extras` |   3.1.3 | Tilemap Extras              |
| `com.unity.burst`             |  1.8.19 | Burst                       |
| `com.unity.collab-proxy`      |  2.12.4 | Version Control Integration |
| `com.unity.collections`       |   1.2.4 | Collections                 |
| `com.unity.ext.nunit`         |   1.0.6 | NUnit                       |
| `com.unity.feature.2d`        |   2.0.1 | 2D Feature Set              |
| `com.unity.ide.rider`         |  3.0.35 | Rider Integration           |
| `com.unity.ide.visualstudio`  |  2.0.22 | Visual Studio Integration   |
| `com.unity.inputsystem`       |  1.14.2 | Input System                |
| `com.unity.mathematics`       |   1.2.6 | Mathematics                 |
| `com.unity.test-framework`    |  1.1.33 | Test Framework              |
| `com.unity.textmeshpro`       |   3.0.7 | TextMeshPro                 |
| `com.unity.timeline`          |   1.7.7 | Timeline                    |
| `com.unity.ugui`              |   1.0.0 | Unity UI                    |
| `com.unity.visualscripting`   |   1.9.4 | Visual Scripting            |

## Unity Built-in Modules

The project also contains `com.unity.modules.*` entries representing Unity built-in modules.

Their use is subject to the applicable Unity licensing terms.

## License Verification

Before publishing:

1. check the exact version of every package actually used;
2. inspect its `LICENSE`, `LICENSE.md`, or `Third Party Notices.md`;
3. check whether package files have been copied into the repository;
4. preserve required copyright and license notices;
5. do not apply the MIT License to files owned by other authors.

Packages may be subject to different terms. Do not assume that every Unity package uses the same license.

## Official Documents

- [Unity Companion License](https://unity.com/legal/licenses/unity-companion-license)
- [Unity Package Distribution License](https://unity.com/legal/licenses/unity-package-distribution-license)
- [Unity Legal Documentation](https://unity.com/legal)
