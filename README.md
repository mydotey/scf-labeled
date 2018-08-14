# scf-labeled: scf library for labeled key scenario

Labeled key like: some-key { label1: label-value1, label2: label-value2 }

Same key with different label series identifies different property.

## Usage

- java: https://github.com/mydotey/scf-labeled/tree/master/java

## Features

- Labeled Key/Property

    key { l1: lv1, l2: lv2, ... }, value

- Auto Fallback

    request.timeout { idc: idc-1, app: app-1 } > request.timeout { idc: idc-1 } > request.timeout

- All features from [scf](https://github.com/mydotey/scf)

## Developers

- Qiang Zhao <koqizhao@outlook.com>
