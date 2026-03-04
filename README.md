<p align="center">
  <img src="https://avatars.githubusercontent.com/u/209633456?v=4" width="160" alt="RedTail Indicators Logo"/>
</p>

<h1 align="center">RedTail Candle Shadows</h1>

<p align="center">
  <b>A visual enhancement indicator for NinjaTrader 8 that draws drop shadows beneath candles.</b><br>
  Adds depth and readability to your chart with configurable shadow offset, opacity, blur, and width.
</p>

<p align="center">
  <a href="https://buymeacoffee.com/dmwyzlxstj">
    <img src="https://img.shields.io/badge/☕_Buy_Me_a_Coffee-FFDD00?style=flat-square&logo=buy-me-a-coffee&logoColor=black" alt="Buy Me a Coffee"/>
  </a>
</p>

---

## Overview

RedTail Candle Shadows renders a subtle drop shadow beneath every candle body and wick on your chart, giving candles a lifted, three-dimensional appearance. It's a purely visual enhancement — no signals, no calculations — designed to improve chart readability, especially when scalping on low timeframes where candles are tightly packed. Rendered entirely with SharpDX for minimal performance impact.

---

## How It Works

The indicator draws offset rectangles behind candle bodies and offset lines behind wicks, using a semi-transparent brush. The shadow is rendered first (behind the actual candle), so the candle sits on top of its shadow naturally. Doji candles with near-zero body height are handled with a minimum 1-pixel body to ensure the shadow remains visible.

---

## Shadow Settings

**Shadow Color** — The color of the shadow. Default: Black. Works well with any chart background; try dark gray on dark themes or medium gray on light themes.

**Shadow Opacity** — Controls how visible the shadow is, from 1% (barely visible) to 100% (fully opaque). Default: 20%. Lower values produce a more subtle, natural-looking shadow.

**Offset X** — Horizontal pixel offset of the shadow. Positive values shift right, negative values shift left. Range: −10 to 10. Default: 2.

**Offset Y** — Vertical pixel offset of the shadow. Positive values shift down, negative values shift up. Range: −10 to 10. Default: 2.

**Shadow Body** — Toggle shadow rendering under candle bodies on or off. Default: on.

**Shadow Wicks** — Toggle shadow rendering under candle wicks on or off. Default: on.

**Wick Opacity Multiplier** — Scales the wick shadow opacity relative to the body shadow. Range: 0.1 to 1.0. Default: 0.6. Since wicks are thinner than bodies, a lower opacity multiplier keeps them from appearing too heavy.

---

## Advanced Settings

**Enable Blur Effect** — Simulates a soft shadow by rendering multiple passes at slightly different offsets with decreasing opacity. Each additional pass shifts the shadow by half a pixel and halves the opacity, creating a diffused edge. Disabled by default as it adds a slight performance cost.

**Blur Passes** — Number of blur passes when blur is enabled. Range: 1 to 3. More passes produce a softer shadow but cost more to render.

**Body Width Adjust** — Adds or subtracts pixels from the shadow body width. Range: −5 to 5. Default: 0. Use positive values if the shadow doesn't fully cover the candle body, or negative values for a tighter shadow.

---

## Performance

The indicator uses SharpDX direct rendering (no NinjaTrader Draw objects), only processes visible bars on screen, and creates/disposes brushes properly on render target changes. With blur disabled, the rendering cost is negligible even on tick charts.

---

## Installation

1. Download the .cs file from the indicator's repository
2. Copy the .cs to documents\Ninja Trader 8\bin\custom\indicators
3. Open Ninja Trader (if not already open) 
4. In control center, go to New --> Ninja Script Editor
5. Expand the Indicator Tree, find your new indicator, double click to open it
6. At the top of the Editor window, click the "Compile" button
7. That's it!

---

<p align="center">
  <a href="https://buymeacoffee.com/dmwyzlxstj">
    <img src="https://img.shields.io/badge/☕_Buy_Me_a_Coffee-Support_My_Work-FFDD00?style=for-the-badge&logo=buy-me-a-coffee&logoColor=black" alt="Buy Me a Coffee"/>
  </a>
</p>
