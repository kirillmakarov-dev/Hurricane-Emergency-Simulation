from __future__ import annotations

import math
import wave
from pathlib import Path

import numpy as np


SAMPLE_RATE = 44_100
DURATION = 58.5
BPM = 112
BEAT = 60.0 / BPM
RNG = np.random.default_rng(20260913)


def note_frequency(midi: int) -> float:
    return 440.0 * (2.0 ** ((midi - 69) / 12.0))


def add_tone(track: np.ndarray, start: float, duration: float, midi: int, amplitude: float, kind: str = "sine") -> None:
    first = int(start * SAMPLE_RATE)
    count = min(int(duration * SAMPLE_RATE), len(track) - first)
    if count <= 0:
        return

    time = np.arange(count, dtype=np.float64) / SAMPLE_RATE
    phase = 2.0 * math.pi * note_frequency(midi) * time
    if kind == "pluck":
        signal = np.sin(phase) + 0.34 * np.sin(phase * 2.0) + 0.12 * np.sin(phase * 3.0)
        envelope = np.exp(-4.6 * time / max(duration, 0.01))
        attack = np.minimum(1.0, time / 0.012)
        signal *= envelope * attack
    elif kind == "bass":
        signal = np.sin(phase) + 0.22 * np.sin(phase * 0.5)
        envelope = np.minimum(1.0, time / 0.025) * np.minimum(1.0, (duration - time) / 0.08)
        signal *= np.clip(envelope, 0.0, 1.0)
    else:
        signal = np.sin(phase) + 0.18 * np.sin(phase * 2.0)
        attack = np.minimum(1.0, time / 0.35)
        release = np.minimum(1.0, (duration - time) / 0.55)
        signal *= np.clip(attack * release, 0.0, 1.0)

    track[first:first + count] += signal * amplitude


def add_kick(track: np.ndarray, start: float, amplitude: float = 0.32) -> None:
    duration = 0.22
    first = int(start * SAMPLE_RATE)
    count = min(int(duration * SAMPLE_RATE), len(track) - first)
    if count <= 0:
        return
    time = np.arange(count, dtype=np.float64) / SAMPLE_RATE
    phase = 2.0 * math.pi * (55.0 * time + 52.0 * (1.0 - np.exp(-20.0 * time)) / 20.0)
    track[first:first + count] += np.sin(phase) * np.exp(-18.0 * time) * amplitude


def add_hat(track: np.ndarray, start: float, amplitude: float = 0.055) -> None:
    duration = 0.075
    first = int(start * SAMPLE_RATE)
    count = min(int(duration * SAMPLE_RATE), len(track) - first)
    if count <= 0:
        return
    time = np.arange(count, dtype=np.float64) / SAMPLE_RATE
    noise = RNG.normal(0.0, 1.0, count)
    noise[1:] = noise[1:] - 0.92 * noise[:-1]
    track[first:first + count] += noise * np.exp(-45.0 * time) * amplitude


def add_clap(track: np.ndarray, start: float, amplitude: float = 0.11) -> None:
    duration = 0.16
    first = int(start * SAMPLE_RATE)
    count = min(int(duration * SAMPLE_RATE), len(track) - first)
    if count <= 0:
        return
    time = np.arange(count, dtype=np.float64) / SAMPLE_RATE
    noise = RNG.normal(0.0, 1.0, count)
    envelope = np.exp(-25.0 * time) * (0.7 + 0.3 * np.sin(2.0 * math.pi * 31.0 * time) ** 2)
    track[first:first + count] += noise * envelope * amplitude


def main() -> None:
    sample_count = int(DURATION * SAMPLE_RATE)
    music = np.zeros(sample_count, dtype=np.float64)

    progression = [
        ([48, 52, 55, 59], 36),  # Cmaj7
        ([45, 48, 52, 55], 33),  # Am7
        ([41, 45, 48, 52], 29),  # Fmaj7
        ([43, 48, 50, 55], 31),  # Gsus
    ]
    bars = math.ceil(DURATION / (BEAT * 4.0))
    melody_patterns = [
        [72, 76, 79, 76, 74, 76, 79, 83],
        [69, 72, 76, 72, 71, 72, 76, 79],
        [65, 69, 72, 69, 67, 69, 72, 76],
        [67, 72, 74, 72, 71, 74, 76, 79],
    ]

    for bar in range(bars):
        start = bar * BEAT * 4.0
        chord, bass = progression[bar % len(progression)]
        for midi in chord:
            add_tone(music, start, BEAT * 4.05, midi, 0.035, "pad")
        add_tone(music, start, BEAT * 1.55, bass, 0.115, "bass")
        add_tone(music, start + BEAT * 2.0, BEAT * 1.55, bass + 7, 0.09, "bass")

        pattern = melody_patterns[bar % len(melody_patterns)]
        for step, midi in enumerate(pattern):
            add_tone(music, start + step * BEAT / 2.0, BEAT * 0.46, midi, 0.054, "pluck")

        for beat_index in range(4):
            beat_start = start + beat_index * BEAT
            add_kick(music, beat_start, 0.26 if beat_index in (0, 2) else 0.18)
            if beat_index in (1, 3):
                add_clap(music, beat_start, 0.085)
            add_hat(music, beat_start, 0.042)
            add_hat(music, beat_start + BEAT / 2.0, 0.032)

    # Gentle stereo width from a delayed copy, then master fades and limiting.
    delay = int(0.012 * SAMPLE_RATE)
    left = music.copy()
    right = np.zeros_like(music)
    right[delay:] = music[:-delay]
    right += music * 0.72
    left += np.roll(music, -delay) * 0.12
    stereo = np.stack([left, right], axis=1)

    fade_in = int(1.2 * SAMPLE_RATE)
    fade_out = int(2.3 * SAMPLE_RATE)
    stereo[:fade_in] *= np.linspace(0.0, 1.0, fade_in)[:, None]
    stereo[-fade_out:] *= np.linspace(1.0, 0.0, fade_out)[:, None]
    stereo = np.tanh(stereo * 1.35)
    peak = np.max(np.abs(stereo))
    stereo *= 0.86 / max(peak, 1e-9)

    output = Path(__file__).resolve().parents[1] / "assets" / "audio" / "hurricane-ready-original.wav"
    output.parent.mkdir(parents=True, exist_ok=True)
    pcm = (stereo * 32767.0).astype("<i2")
    with wave.open(str(output), "wb") as wav:
        wav.setnchannels(2)
        wav.setsampwidth(2)
        wav.setframerate(SAMPLE_RATE)
        wav.writeframes(pcm.tobytes())
    print(output)


if __name__ == "__main__":
    main()
