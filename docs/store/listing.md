# Cannons — store listing draft (2026-10-08)

Draft for Google Play / CrazyGames. Limits: Play title 30 chars, short description 80, full 4000.
Facts must match the build: 500 levels, password jump, no ads, no data collected (see privacy.html).
UI languages: English + Spanish (since 2026-10-20; auto by browser/system language, toggle in menu).

## Español

**Título:** Cannons: Defensa Pirata

**Descripción corta (≤80):** Arrastra, fusiona y dispara: frena a los piratas en 500 niveles de estrategia.

**Descripción completa:**
Los piratas avanzan por cinco columnas y solo tus cañones los separan de la orilla.

Cada ronda recibes un cañón nuevo. Colócalo en la columna correcta, muévelo cuando la amenaza cambia de lado o fusiónalo con otro para sumar su fuerza. Los piratas más duros solo caen si preparaste la fusión a tiempo.

- 500 niveles diseñados y probados, con curva de dificultad en arcos: respiro, subida y un pico final.
- Partidas cortas: un nivel dura un par de minutos.
- Fácil de aprender, difícil de dominar: arrastrar y soltar con un dedo o el ratón.
- Contraseñas para saltar a cualquier nivel desbloqueado.
- Sin anuncios, sin registro, sin conexión necesaria.

## English

**Title:** Cannons: Pirate Defense

**Short description (≤80):** Drag, merge and fire: stop the pirates across 500 strategy levels.

**Full description:**
Pirates march up five columns and only your cannons stand between them and the shore.

Every round you get a new cannon. Place it in the right column, move it when the threat switches sides, or merge it with another to stack their power. The toughest pirates only fall if you prepared the merge in time.

- 500 play-tested levels with a difficulty curve in arcs: breather, climb and a final peak.
- Short sessions: a level takes a couple of minutes.
- Easy to learn, hard to master: one-finger or mouse drag and drop.
- Passwords to jump to any unlocked level.
- No ads, no sign-up, plays offline (mobile/PC builds; on the web it needs the page loaded).

## Assets still needed (Play)
- Icon 512x512: `graphics/icon_512.png` (from Assets/UI/iconApp.png, inner square — Play masks it itself).
- Feature graphic 1024x500: `graphics/feature_1024x500.png` (menu art, right part — the left has leftover ghost text).
  Both regenerable with PIL from Assets/UI (2026-10-08).
- 2-8 phone screenshots, landscape (16:9, min 320 px side; 1920x1080 works).
- Category: Strategy (or Puzzle). Content rating questionnaire: no violence beyond cartoon, no data collection.
- Privacy policy URL: https://alejandrozumbado.github.io/cannons-build/privacy.html (after publishing docs/store/privacy.html there).

## CrazyGames Basic Launch (2026-11-12)
- **Title:** Cannons: Pirate Defense. **Developer:** Drixwave.
- **Category:** Strategy (tags: tower defense, merge, pirates, puzzle, cannon).
- **Languages:** English, Spanish.
- **Orientation:** landscape. **Mobile:** yes (single touch).
- **Controls:** Mouse / touch: drag the new cannon from the barrel onto a column. Drop it on another cannon to merge (damage adds up). Drag a placed cannon to move it to another column. Esc = pause.
- **Description:** use "English > Full description" above (web: drop the "plays offline" bullet).
- **Covers:** `graphics/crazygames/cover_1920x1080.png`, `cover_800x1200.png`, `cover_800x800.png` (regenerate: `python docs/store/make_covers.py`).
- **Videos (pending, user records):** 2 clips 15-20 s, no sound: 16:9 and 2:3.
- **Build:** zip of the WebGL build folder (`index.html` at zip root), TestLevels empty.
