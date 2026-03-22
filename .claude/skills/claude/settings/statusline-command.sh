#!/usr/bin/env bash
input=$(cat)

model=$(echo "$input" | jq -r '.model.display_name // empty')
used=$(echo "$input" | jq -r '.context_window.used_percentage // empty')
remaining=$(echo "$input" | jq -r '.context_window.remaining_percentage // empty')
cur_in=$(echo "$input" | jq -r '.context_window.current_usage.input_tokens // empty')
cur_out=$(echo "$input" | jq -r '.context_window.current_usage.output_tokens // empty')
rate_five=$(echo "$input" | jq -r '.rate_limits.five_hour.used_percentage // empty')
rate_resets_at=$(echo "$input" | jq -r '.rate_limits.five_hour.resets_at // empty')
rate_week=$(echo "$input" | jq -r '.rate_limits.seven_day.used_percentage // empty')
rate_week_resets_at=$(echo "$input" | jq -r '.rate_limits.seven_day.resets_at // empty')

output=""

# Project folder name
proj_dir=$(basename "$(pwd)")
bold='\033[1m'
reset='\033[0m'
output=$(printf "📁 ${bold}[%s]${reset}" "$proj_dir")

# Return ANSI color code based on used_percentage
# 0-49: white, 50-65: yellow, 65-80: orange, 80+: red
gauge_color() {
  local pct=$1
  if [ "$(echo "$pct >= 80" | bc -l)" -eq 1 ]; then
    printf '\033[31m'        # red
  elif [ "$(echo "$pct >= 65" | bc -l)" -eq 1 ]; then
    printf '\033[38;5;208m'  # orange
  elif [ "$(echo "$pct >= 50" | bc -l)" -eq 1 ]; then
    printf '\033[33m'        # yellow
  else
    printf '\033[37m'        # white
  fi
}

# Model name
if [ -n "$model" ]; then
  output="${output} │ $(printf "🤖 [\033[1m%s\033[0m]" "$model")"
fi

# Context bar + percentages
if [ -n "$used" ]; then
  total=10
  filled=$(printf "%.0f" "$(echo "$used * $total / 100" | bc -l)")
  [ "$filled" -gt "$total" ] && filled=$total
  empty=$((total - filled))

  color=$(gauge_color "$used")
  reset='\033[0m'
  bold='\033[1m'

  filled_bar=""
  for i in $(seq 1 "$filled"); do filled_bar="${filled_bar}▓"; done
  empty_bar=""
  for i in $(seq 1 "$empty");  do empty_bar="${empty_bar}░"; done

  bar="${color}${filled_bar}${reset}${empty_bar}"

  pct=$(printf "%.0f" "$used")

  ctx_part=$(printf "🧠 ${bold}CONTEXT:${reset} %b ${bold}%s%%${reset}" "$bar" "$pct")

  [ -n "$output" ] && output="${output} │ " || output=""
  output="${output}${ctx_part}"
fi

# Rate limit (5-hour), only when present
if [ -n "$rate_five" ]; then
  rate_total=10
  rate_filled=$(printf "%.0f" "$(echo "$rate_five * $rate_total / 100" | bc -l)")
  [ "$rate_filled" -gt "$rate_total" ] && rate_filled=$rate_total
  rate_empty=$((rate_total - rate_filled))

  rate_color=$(gauge_color "$rate_five")
  rate_reset='\033[0m'
  bold='\033[1m'

  rate_filled_bar=""
  for i in $(seq 1 "$rate_filled"); do rate_filled_bar="${rate_filled_bar}▓"; done
  rate_empty_bar=""
  for i in $(seq 1 "$rate_empty");  do rate_empty_bar="${rate_empty_bar}░"; done

  rate_bar="${rate_color}${rate_filled_bar}${rate_reset}${rate_empty_bar}"

  rate_pct=$(printf "%.0f" "$rate_five")
  rate_part=$(printf "⚡ ${bold}DAILY:${rate_reset} %b ${bold}%s%%${rate_reset}" "$rate_bar" "$rate_pct")

  # Append resets_at as "↺XhYm" if available
  if [ -n "$rate_resets_at" ]; then
    now=$(date +%s)
    diff=$((rate_resets_at - now))
    if [ "$diff" -gt 0 ]; then
      diff_h=$((diff / 3600))
      diff_m=$(( (diff % 3600) / 60 ))
      if [ "$diff_h" -gt 0 ]; then
        resets_str="↺${diff_h}h${diff_m}m"
      else
        resets_str="↺${diff_m}m"
      fi
      rate_part="${rate_part} ${resets_str}"
    fi
  fi

  output="${output} │ ${rate_part}"
fi

# Weekly rate limit (7-day), only when present
if [ -n "$rate_week" ]; then
  week_total=10
  week_filled=$(printf "%.0f" "$(echo "$rate_week * $week_total / 100" | bc -l)")
  [ "$week_filled" -gt "$week_total" ] && week_filled=$week_total
  week_empty=$((week_total - week_filled))

  week_color=$(gauge_color "$rate_week")
  week_reset='\033[0m'
  bold='\033[1m'

  week_filled_bar=""
  for i in $(seq 1 "$week_filled"); do week_filled_bar="${week_filled_bar}▓"; done
  week_empty_bar=""
  for i in $(seq 1 "$week_empty");  do week_empty_bar="${week_empty_bar}░"; done

  week_bar="${week_color}${week_filled_bar}${week_reset}${week_empty_bar}"

  week_pct=$(printf "%.0f" "$rate_week")
  week_part=$(printf "${bold}WEEKLY:${week_reset} %b ${bold}%s%%${week_reset}" "$week_bar" "$week_pct")

  # Append resets_at as "↺XdYh" or "↺XhYm" if available
  if [ -n "$rate_week_resets_at" ]; then
    now=$(date +%s)
    diff=$((rate_week_resets_at - now))
    if [ "$diff" -gt 0 ]; then
      diff_d=$((diff / 86400))
      diff_h=$(( (diff % 86400) / 3600 ))
      diff_m=$(( (diff % 3600) / 60 ))
      if [ "$diff_d" -gt 0 ]; then
        week_resets_str="↺${diff_d}d${diff_h}h"
      elif [ "$diff_h" -gt 0 ]; then
        week_resets_str="↺${diff_h}h${diff_m}m"
      else
        week_resets_str="↺${diff_m}m"
      fi
      week_part="${week_part} ${week_resets_str}"
    fi
  fi

  output="${output} │ ${week_part}"
fi

# Last call token info (rightmost)
if [ -n "$cur_in" ] && [ -n "$cur_out" ]; then
  bold='\033[1m'
  reset='\033[0m'
  output="${output} │ $(printf "💬 ${bold}LAST:${reset}") ↑${cur_in} ↓${cur_out}"
fi

[ -n "$output" ] && printf "%b" "$output"
