#!/usr/bin/env python3
"""Offline repository checks. Standard library only; never advances project state."""
import argparse
import csv
import hashlib
import json
import re
import subprocess
import xml.etree.ElementTree as ET
from datetime import datetime
from pathlib import Path
from urllib.parse import unquote, urlsplit
from zoneinfo import ZoneInfo

ROOT = Path(__file__).resolve().parents[2]
AUTHORIZED_PART = 'P02.5'
# D024/D031: exact reviewed reference, generated runtime art and actual Unity render evidence.
REVIEWED_REFERENCE_ASSETS = {'docs/references/nard-board-primary.png': 'ac2b6c31cd386a2c3b49a5b27cc6db6c66be5e7c1ce3d128fb93143efb2f40c3', 'docs/screenshots/P02.4-editor-gameview.png': 'f8faec3b95c71e7c514d303544068086d7a19ee50c8d7e03880bb99ea463640e', 'docs/screenshots/P02.4-undo-drag-long.png': '0ee87615fa6ecc4f43437c704483f464617750b192773d4b8c5d5f0aea16648f', 'docs/screenshots/P02.4-undo-drag-short.png': 'ec7f4361335328a02788554e011d2ab69f376601e3730242ada568854604acc5', 'docs/screenshots/P02.4-undo-moved-long.png': '73b046b42a852c2318ca32b68923c24610860b8009b6ab5999e2fc04c8d35343', 'docs/screenshots/P02.4-undo-moved-short.png': '6e9df17f367440a7accfd0bda6d2151ecf675e7e1f225f688b7c124fd30d2f28', 'docs/screenshots/P02.4-undo-restored-long.png': '379bb5170013c3e739661a7e7856aa234b6728e6ccd3c21ffde4e95484b61fa3', 'docs/screenshots/P02.4-undo-restored-short.png': '7e59acad01d9fa4d8986f77bfda9919602a76e572bc33dde264e2f099716657a', 'docs/screenshots/P02.4-undo-restored.png': '379bb5170013c3e739661a7e7856aa234b6728e6ccd3c21ffde4e95484b61fa3', 'docs/screenshots/P02.5-editor-gameview.png': '56f5c86f3c564ece6fdb5c6d3cdbc7d21c48cccf784d13122d91e50886bd6a2d', 'docs/screenshots/P02.5-layout-landscape.png': '22596eb4c5eac2eb392d477fd9a43092fec06397bc261d0e7b8e7f8212eb915e', 'docs/screenshots/P02.5-layout-portrait-moving.png': 'f592d8fab2518f67d56839c70384cf02618a3eac68f5f9a035d25f68543943f5', 'docs/screenshots/P02.5-layout-portrait-opening.png': '67f9b4f2b149ac1fccb7fce1a99e29a4ff3d8aea3a41db135bc97750d14f5996', 'docs/screenshots/P02.5-layout-portrait-selection.png': '36363fccab446b52ac197ab148b3db2fc3ead26b7b795a95fd646a3e48ff6f10', 'docs/screenshots/P02.5-layout-portrait.png': '397c83eae030b56a25544961c74a5493ec836272bd079577cdf3cdd6b7a219a7', 'docs/screenshots/P02.5-layout-short-bar.png': '596168aef031ff3219eb540a0f66037eb0232ea6b4dc2a192edb088582fe2278', 'docs/screenshots/P02.5-layout-short-off.png': '6b37c6e432717563247c773e52a56a24d443272f31504466ef11440b5ca42aeb', 'docs/screenshots/P02.5-layout-tablet.png': '08d9968f071e97e91ea59e2453e3095a52a8b7c8d5ca76524c8ee740802b506b', 'docs/screenshots/P02.5-layout-tall-safe.png': '7b66fd99d1c3945858843638e7e8aefd56013f6071fee083cf65595de5aa40f8', 'docs/screenshots/P02.5-layout-wide-safe.png': '1e560d23fe0ce7282695cd581557f7f147be99c685696313f47e963d7c97efcf', 'docs/screenshots/P02.5-portrait-stack-1-320x568.png': 'ea53e8595374df029b53e770f66e7ea5a0835b28687d94ad1a7d56d5328d181e', 'docs/screenshots/P02.5-portrait-stack-1-540x960.png': 'b1a81a6bd4992a806e4e67f1eec093a2a1bfb529e524a2053dbb4c881a0cb81e', 'docs/screenshots/P02.5-portrait-stack-10-320x568.png': '47aea624579ee4b5e8fd721fcd4d5b13b80cd8484b0ddcab6eda99d976c616eb', 'docs/screenshots/P02.5-portrait-stack-10-540x960.png': '9e2efc8bd572d26b2fc42e7750ee86cd5430425b83013c241f5a35a8631e29df', 'docs/screenshots/P02.5-portrait-stack-15-320x568.png': '6f02c1e6b1639f5bac100bfa877fc525b510788ad253d8e1fbab249237bc7d2f', 'docs/screenshots/P02.5-portrait-stack-15-390x844.png': '4ae17c509cc2fdeb9ca4fd3e243fae7d5944aaf91f163f0fa888f6710cd6c234', 'docs/screenshots/P02.5-portrait-stack-15-540x960.png': 'a0cfc2c3f269106db0e37744863d961237cfdccea4fab26b98e48152cf78c4c5', 'docs/screenshots/P02.5-portrait-stack-15-720x1600.png': 'b949f12e00fcd3bd30472a68c4032b54e143fbcf1bfe710ba9b3e705573673c3', 'docs/screenshots/P02.5-portrait-stack-15-bar.png': '546d39a55b52a0e2f31e3cdcd060cf7f7ff01b49bedbc3385f3de3bbb04a8bc7', 'docs/screenshots/P02.5-portrait-stack-15-off.png': 'ea49aa72a9555e8a6add611e9c9fad4881d02b02d7081ecf3a47c2d8139ae29d', 'docs/screenshots/P02.5-portrait-stack-2-320x568.png': '9edb3c07859509513245ec457917f7483449203fb24cdcc1a20dc14613b43418', 'docs/screenshots/P02.5-portrait-stack-2-540x960.png': '93da46c030758aac6cde752d64fa5d08f1f1f66eb305756f08c320d3d2868101', 'docs/screenshots/P02.5-portrait-stack-5-320x568.png': '731d80dff6e0e60590ec7036e38cc52b7b3ca97d4b547c78903e2aba0950f0b0', 'docs/screenshots/P02.5-portrait-stack-5-540x960.png': 'fee3cf108a4c17386cc2e332e6b728188cbbf00f518274b77eac6616199f3ed1', 'docs/screenshots/P02.5-portrait-unified-320x568.png': '897e229d051c7e1ee71e553da63a206a0368c75e64ac99da0ef8f538439dc72a', 'docs/screenshots/P02.5-portrait-unified-390x844.png': '2e1e0987dc0bd234512d9708d1cad9969421574811311e8d998d2435e8b9f3da', 'docs/screenshots/P02.5-portrait-unified-540x960.png': 'eb2624f6b2dd3ce2b089fbdd8bc540653ac9b9d45db00e32b005f4be5cd48f25', 'docs/screenshots/P02.5-portrait-unified-720x1600.png': '332230e59187ec2f1ff017f41c543b45a3f51e6bd59463a869d1d525c5f7b4d2', 'docs/screenshots/P02.5-portrait-unified-long.png': '163fe238c9310840ce2682686a5c99ed4185d2777a6dc0cf6de5a7d17dea4057', 'unity/Assets/Game/Presentation/Resources/NardBoard.png': '4184aaf403d974bcf5d26f33f3ce676086e467684cdc8944590e2f05329e00fa', 'unity/Assets/Game/Presentation/Resources/NardCheckers.png': 'd236ef73b2ac0e5314caf9e15419ff3ee79877c2e189575cbe89a0229bf46e69'}


# Exact real Unity render evidence, reviewed after the D031 visual amendment.


def allowed_file_size(name, data):
    if name in REVIEWED_REFERENCE_ASSETS:
        return len(data) <= 4 * 1024 * 1024 and hashlib.sha256(data).hexdigest() == REVIEWED_REFERENCE_ASSETS[name]
    return len(data) <= 1024 * 1024

ENGINE_BASELINE = {'unity/Assets/Game/Interaction/TapSelection.cs': '069b440905d0240cc5ddc549cdab4b1c12d85b1cfd6c69020bb442e18d1fd9ba',
 'unity/Assets/Game/Interaction/TapSelection.cs.meta': 'ffaac493b3d24a5621e6c8ee9a9852e1b88fee5a4ee04ae794c5ac2b5cad7a0c',
 'unity/Assets/Game/Rules/LongNardeRules.cs': '48e29e9ac9231d198a04bea9026e434fb377763a7632ed3627bcfa5d08b96a07',
 'unity/Assets/Game/Rules/LongNardeRules.cs.meta': 'c57e59400cd9b27753e425648c29590da7c29597ff06ef5a4d4721f128d47796',
 'unity/Assets/Game/Rules/LongPosition.cs': 'e5e084c9ec79481adbc1e36d6314fc5ddef19f08a37eb6924d10a24ee2587c5c',
 'unity/Assets/Game/Rules/LongPosition.cs.meta': 'eec70ed4adca23273a4f9074065777e7087dc08dcc49f2aa7a38a4e09b1fbf0a',
 'unity/Assets/Game/Rules/Match.meta': '6ae0340b000cef09d0906a22aaacd6876667a1fa40ef3ab5f4af22c7866af5fb',
 'unity/Assets/Game/Rules/Match/MatchContracts.cs': '3605bac9cdfcea0f570a06a27a1924e202a63196b0a5abc08ddf6cb1ba03193c',
 'unity/Assets/Game/Rules/Match/MatchContracts.cs.meta': 'ac1dde11fe14a0fe57f1b49634bfd40808d7b471ce1a6709e19ead3dccad48e0',
 'unity/Assets/Game/Rules/Match/MatchSeries.cs': '73c6f7e10c987a628d0cb34bcdb2a5efaa0cefa39c72e5ef3f0686cce9fd577c',
 'unity/Assets/Game/Rules/Match/MatchSeries.cs.meta': '00f4a4977aeb2509f3d87666527e88fe233344ede7f370400ba9d0ef58953101',
 'unity/Assets/Game/Rules/Match/MatchSession.cs': '9046ffdb3c4e592eada4fc793cbfd8458089afa31c50da8c25fdcbeb071a971e',
 'unity/Assets/Game/Rules/Match/MatchSession.cs.meta': '0cbec4e196ec200e31b0cadef5be578e6436b8b99795b48da8adeccbb95c6a92',
 'unity/Assets/Game/Rules/Match/Nard.Match.asmdef': '344f85f556396c5649367ac5f7f1a84a7572082c52afa8bf020f42ccdcf86a60',
 'unity/Assets/Game/Rules/Match/Nard.Match.asmdef.meta': '21c266ef918c310d865c8a915c62a5e465f92376b6e700795b95d357d8cc26d9',
 'unity/Assets/Game/Rules/Nard.Rules.Long.asmdef': '5a679457f5465cde54ed4495b7a5b7c7d92249187b5b7b2ceff3aa61cfb66b0d',
 'unity/Assets/Game/Rules/Nard.Rules.Long.asmdef.meta': 'c75cbae2d8afecb19d0e42e6593bc7582b10eb77266e55f6db374682ccfcc589',
 'unity/Assets/Game/Rules/Short.meta': '71983c16886d3d1f678e8a73ba7dee6e012002b0d4785cdfae11625de65641a8',
 'unity/Assets/Game/Rules/Short/Nard.Rules.Short.asmdef': '56301d3301c4a749004be25e515afdfdc9c52bcb99bcfcdc761972d3af49819a',
 'unity/Assets/Game/Rules/Short/Nard.Rules.Short.asmdef.meta': 'a2c68bac09bce3ee0cf779a6552888e30e65a354e944594928ea08c83f621ad4',
 'unity/Assets/Game/Rules/Short/ShortNardeRules.cs': '8eea41858f56fc1d53c56077edf95c224e091a37c0c81fe3f5e1b64ec0b03904',
 'unity/Assets/Game/Rules/Short/ShortNardeRules.cs.meta': 'b4b1488bd3ac37b203962614ccc82d99331ee6295eb40e2760cf653bf2b0f84b',
 'unity/Assets/Game/Rules/Short/ShortPosition.cs': '8b59b463c250a73874dd1e2a32159e4d8d0b839172aeffb057b76a10a886dd9d',
 'unity/Assets/Game/Rules/Short/ShortPosition.cs.meta': '2f05a5cf1e0b651af0ea91cbeb5a49533e1a4708b1d998eefbd53db4f87ddbb2',
 'unity/Assets/Game/Tests/BoardEditor.meta': '831a8ae3136521ab7dffeb26aa3f26c876e9e5acfa620d77ff14d64c6d24eab3',
 'unity/Assets/Game/Tests/BoardEditor/BoardProjectionTests.cs': 'c09c005778d73f492c25104589255e169d0285f2ff5f889d14ec42290ee60403',
 'unity/Assets/Game/Tests/BoardEditor/BoardProjectionTests.cs.meta': 'cdca0ededdd254b8b62f12a487433c217638b274fd16f3bac4501c066eb398b9',
 'unity/Assets/Game/Tests/BoardEditor/Nard.Tests.BoardEditor.asmdef': '3b9ba69d4729ee6ddd5ec625a9581b699176c3b580513ffd096f3c0863ff0f5a',
 'unity/Assets/Game/Tests/BoardEditor/Nard.Tests.BoardEditor.asmdef.meta': 'd0c4845834d0174773cb2ddfffc557fa03822d5480492840c4afbd186bbfa549',
 'unity/Assets/Game/Tests/BoardPlayMode.meta': '6accef8e28d7da1b9f9d5a705a0657ed76231e318a22371d923d75cb86c8ab94',
 'unity/Assets/Game/Tests/BoardPlayMode/BoardSceneTests.cs': '2efb14ebc5890a35a58e1396c42c3c898253e1d2d3e2b775940307606734eace',
 'unity/Assets/Game/Tests/BoardPlayMode/BoardSceneTests.cs.meta': '4e21d0299df8656e71ec79912bfcb365150ef67e8a0bb83445361079f8d538e9',
 'unity/Assets/Game/Tests/BoardPlayMode/Nard.Tests.BoardPlayMode.asmdef': '572d94065bd454f32257e293f93e85c640466ae7de4137cfdff41806869771f9',
 'unity/Assets/Game/Tests/BoardPlayMode/Nard.Tests.BoardPlayMode.asmdef.meta': '440f699cf7dc604585698e656267c0048d10b72378251b02b09d137a910c12c0',
 'unity/Assets/Game/Tests/EditMode.meta': 'db138cbdf888b9e2905cb074127852aca61619f596f866c265c037b4043e0819',
 'unity/Assets/Game/Tests/EditMode/FoundationTests.cs': '5dc69aca84e6dc0f687318e8e1ac13a520f4b1d201eac0c48a9b2b2f9fd95f7f',
 'unity/Assets/Game/Tests/EditMode/FoundationTests.cs.meta': '746763aee149b64682e720c85ef0cc2d0ad4fb3b844aefda3e61d10fc237b763',
 'unity/Assets/Game/Tests/EditMode/Nard.Tests.EditMode.asmdef': '1f63c86ff2f28d4f605275a36b1809b8463cd85ef76c0c1554e3ce605a912cb0',
 'unity/Assets/Game/Tests/EditMode/Nard.Tests.EditMode.asmdef.meta': '731d06adb2964a1d3f09808390f0c68ee5325fbbe9185358fcbcfeafcd924316',
 'unity/Assets/Game/Tests/Match.meta': '09d192036a4626ced58478d6e4ce0c5acdd51c0c1623b8ce961d141efd32184a',
 'unity/Assets/Game/Tests/Match/MatchTests.cs': '7fde7bf4fe77108cd4c2a7858072bbf9765fae3f220f0bcca2ec9ced6fc33215',
 'unity/Assets/Game/Tests/Match/MatchTests.cs.meta': 'fa9628ad76b958f20374f8ff015559f754aab9daed2a6e48628f119d08b62668',
 'unity/Assets/Game/Tests/Match/Nard.Match.Tests.asmdef': '02bbff6a52a4e275e816a30bf557d2e74eee53956241d4ec020b69be736033e2',
 'unity/Assets/Game/Tests/Match/Nard.Match.Tests.asmdef.meta': '5ba545b5680503d9b3761b45770851af2a1375a7a9dcb948299c038023f5f42f',
 'unity/Assets/Game/Tests/PlayMode.meta': 'cba653964e8bebf1da95ff3cfc382042f182c30c1932faf00c7993b4a9220841',
 'unity/Assets/Game/Tests/PlayMode/BootstrapSmokeTests.cs': 'dececb7d596657ca1ff837a3d8aa78b73e9bc8547d40c1cb8cdfdef6ce575d61',
 'unity/Assets/Game/Tests/PlayMode/BootstrapSmokeTests.cs.meta': 'f576b439ddf37450155e8bbc47c82150590f37dc6d59601b31903d6b0d45933c',
 'unity/Assets/Game/Tests/PlayMode/Nard.Tests.PlayMode.asmdef': '7e7ebbcc7e939a7233279d2406cb1eeed50dcca50da4177d06501baaf55a669d',
 'unity/Assets/Game/Tests/PlayMode/Nard.Tests.PlayMode.asmdef.meta': 'a055ac57f24c98191970b13b84919ae7685eed8289273b2b111bc30f1d845d30',
 'unity/Assets/Game/Tests/Rules.meta': '0c75af4036e440e9e4c3614ea89e09d2b99f39086b581d2748052de8144cdec3',
 'unity/Assets/Game/Tests/Rules/LongRulesTests.cs': '14c4dbdc39f4cc3c0fb965c934afc747e7c89b146ee6e01eba36cec4841da362',
 'unity/Assets/Game/Tests/Rules/LongRulesTests.cs.meta': '10bb0adfc3b5632f948cbe731f4fecbcc1a64f28cfba7755d6530f195ab504d8',
 'unity/Assets/Game/Tests/Rules/Nard.Rules.Long.Tests.asmdef': '4ce440ca99c8abfbbf8a227ce574081d9a1866d8c0887a83759edba2cdaa460b',
 'unity/Assets/Game/Tests/Rules/Nard.Rules.Long.Tests.asmdef.meta': 'bc897bb9f6c148aa933d9bf1f3572d3d09c154ec4844e4813cf1b7a24b205076',
 'unity/Assets/Game/Tests/ShortRules.meta': '8b13721a0de1b35446821d5a680aba1cec347efb3c4dc8be7fb3c5cd8b19c417',
 'unity/Assets/Game/Tests/ShortRules/Nard.Rules.Short.Tests.asmdef': '670f77d2cf346383a470ee8f77fb8cbb3aade54898e32ccb4d9ee014e71ae837',
 'unity/Assets/Game/Tests/ShortRules/Nard.Rules.Short.Tests.asmdef.meta': 'd1d5912208f481e650861b66095c490f107ef6749918154a3cf6ef0d034d8ed0',
 'unity/Assets/Game/Tests/ShortRules/ShortRulesTests.cs': 'd8f26c34249fd0aa5b0c897a38bd3edd988639298058311ff4a6205becae50d1',
 'unity/Assets/Game/Tests/ShortRules/ShortRulesTests.cs.meta': '10897e52aee127d3906266fad97890189fbf49b4c1310d5d0049c6c656ad688f',
 'unity/Assets/Game/Tests/TapEditor.meta': '34014c84ca76f93e7a7c484504dd431a51329ff285d5aed288371d8ae171c332',
 'unity/Assets/Game/Tests/TapEditor/Nard.Tests.TapEditor.asmdef': '539c09b1bdbcb0a648f244a3f98035a38a4256611ace8eb3261f4b1b623fc956',
 'unity/Assets/Game/Tests/TapEditor/Nard.Tests.TapEditor.asmdef.meta': '1573f1c7d6419c6ec3e25bf54e07df12fc0a3844d24063050f728079639a8ed2',
 'unity/Assets/Game/Tests/TapEditor/TapSelectionTests.cs': '2197e481d5330de702f38c252452997e825f58b6cdab1049351d971a1b53650d',
 'unity/Assets/Game/Tests/TapEditor/TapSelectionTests.cs.meta': '9abcc46f40f004e3e524d9c8932d9d5a6ff998414526a68fa7eee93275d13563',
 'unity/Assets/Game/Tests/TapPlayMode.meta': '4cfdff75746c223b51f6a1c03d4b33640c29bab9c224a311523895413313a14f',
 'unity/Assets/Game/Tests/TapPlayMode/Nard.Tests.TapPlayMode.asmdef': '696a5f0c35f8de4b4315eb9bbe621cc5e814e46a958afab5b643b16dbaa1fab5',
 'unity/Assets/Game/Tests/TapPlayMode/Nard.Tests.TapPlayMode.asmdef.meta': 'aef63235e585c0879839d72513355eac7a5714c643d6ff38517efa1476c0076f',
 'unity/Assets/Game/Tests/TapPlayMode/TapSceneTests.cs': '3d15e3a143efd2e3157df11d99d01b8e91bbbf700eac4f8d469d6a4d48f0a855',
 'unity/Assets/Game/Tests/TapPlayMode/TapSceneTests.cs.meta': '6bb75464b12572bdf80b183f371053afb8d8baebf783d0630a3ffa5f6f2ea8f4'}

ENGINE_BASELINE.update({'unity/Assets/Game/Interaction/LocalDiceSource.cs': '834163ea22ab06943f33fc25bf2072987eab400915871ad7bce8b18cb0a158c4',
 'unity/Assets/Game/Interaction/LocalDiceSource.cs.meta': '4bb142ea17011b864989b38d59477f9de0b5e9af79b7f53d53eacdcba13a74e7',
 'unity/Assets/Game/Tests/TurnEditor.meta': '65cd8ad91b7f2747d6ac249710e5f244103a3c19f1161e03aba59b0ff4881be5',
 'unity/Assets/Game/Tests/TurnEditor/LocalTurnTests.cs': '1cd8d9786e43b8b092f181f4325052daebbb23968b7a8824bc722a97cd287700',
 'unity/Assets/Game/Tests/TurnEditor/LocalTurnTests.cs.meta': '7287e9d706887e0cade491dad970f6395e764b7f4804f9ec6e5592dc1177d1a0',
 'unity/Assets/Game/Tests/TurnEditor/Nard.Tests.TurnEditor.asmdef': '3a9b895af948535f93d6aa16d73234f86d9528cbcf457e7927c3fb51910ab815',
 'unity/Assets/Game/Tests/TurnEditor/Nard.Tests.TurnEditor.asmdef.meta': '259984b763f0cc88dff9a9d286fb70e3d34c116c7d2967419df2c43ac391ac6c',
 'unity/Assets/Game/Tests/TurnPlayMode.meta': '489442560341f8ba9748542c6f865672dae74aea9254c3c9be302f30a0cd85bf',
 'unity/Assets/Game/Tests/TurnPlayMode/Nard.Tests.TurnPlayMode.asmdef': '09d8038bb14035e0da70288aa5a892c130827e910ea2e67b899a3702598342b5',
 'unity/Assets/Game/Tests/TurnPlayMode/Nard.Tests.TurnPlayMode.asmdef.meta': 'b4acedf2a78ac937630325e4404231fc407bd26b4092f62fa7aee7930d2b216a',
 'unity/Assets/Game/Tests/TurnPlayMode/TurnSceneTests.cs': 'd551217946586820b46dd5b8d4bbbaa96191df9d960a7533a80895c94bb0b392',
 'unity/Assets/Game/Tests/TurnPlayMode/TurnSceneTests.cs.meta': 'e2fad3591261c4305c2908d88547d7d952377be5afb91982978a0aac41e647ab'})
ENGINE_BASELINE.update({'unity/Assets/Game/Interaction/LocalDiceSource.cs': '834163ea22ab06943f33fc25bf2072987eab400915871ad7bce8b18cb0a158c4',
 'unity/Assets/Game/Interaction/LocalDiceSource.cs.meta': '4bb142ea17011b864989b38d59477f9de0b5e9af79b7f53d53eacdcba13a74e7',
 'unity/Assets/Game/Interaction/LocalTurnLoop.cs': '3540aab428af2b71b380f15e9216c465d59577e3c60ff784cf43e719e5771034',
 'unity/Assets/Game/Interaction/LocalTurnLoop.cs.meta': '24ae666bff9880421777153ace346098a6c94ee113d4fc799993aa6d5e6b6dec',
 'unity/Assets/Game/Interaction/Nard.Interaction.asmdef': 'c1dd78dd748ab53b1fcee156b9c6811a519e69d0ad937f49fbdc28847532d7e9',
 'unity/Assets/Game/Interaction/Nard.Interaction.asmdef.meta': '0c787465f58f02092aff48b70819c89f4c2d565bb5c670042de6f4bee7b50e9b',
 'unity/Assets/Game/Interaction/TapSelection.cs': '069b440905d0240cc5ddc549cdab4b1c12d85b1cfd6c69020bb442e18d1fd9ba',
 'unity/Assets/Game/Interaction/TapSelection.cs.meta': 'ffaac493b3d24a5621e6c8ee9a9852e1b88fee5a4ee04ae794c5ac2b5cad7a0c',
 'unity/Assets/Game/Interaction/UndoPreview.cs': '8b06546e64ed738c1c880b92c1121aeeadfcc3fe08f0b6c1fcd1239b819a611f',
 'unity/Assets/Game/Interaction/UndoPreview.cs.meta': '29be8e3cdce3385f403193f5d1994b09aa86da20b2eeec18e7c07fe529e9031a',
 'unity/Assets/Game/Tests/UndoEditor.meta': '149fa23848b04192ad0d88f2893900cdc9aae65ba03e52ae1b5983d4f9292ba5',
 'unity/Assets/Game/Tests/UndoEditor/Nard.Tests.Undo.asmdef': '0ea95892240be544473ffa0bcac59bf6cb224b545a8affbbd22934d2499532a6',
 'unity/Assets/Game/Tests/UndoEditor/Nard.Tests.Undo.asmdef.meta': 'f94b4e08dd4bcdf39283c9533a23b635d638adf3957d0efaaaf7c5d6e282f207',
 'unity/Assets/Game/Tests/UndoEditor/ReferenceSkinTests.cs': 'a1fb0af982249dba037df8937f9c8de500f73b7bb77e8867d8d2f1fd307f7408',
 'unity/Assets/Game/Tests/UndoEditor/ReferenceSkinTests.cs.meta': '4540d82b2dcbf208cf6ea360b0a2873684c9ab4c22d1b4d0db8fc80bec907257',
 'unity/Assets/Game/Tests/UndoEditor/UndoRegressionTests.cs': 'd65401aeaa4f7ab3f1b9ac5277066fa1d25dc3b6998ee9631b47d4752a5a27e1',
 'unity/Assets/Game/Tests/UndoEditor/UndoRegressionTests.cs.meta': '3c197b21ab8a907f3cfcd0bfa12e3ffbf79a43fb0be14c1e0d8e5bc49be3de89',
 'unity/Assets/Game/Tests/UndoEditor/UndoTests.cs': '99cd550a05aadac023f8d566af0a3111e5551177d8021aeb9a456043b75ae33c',
 'unity/Assets/Game/Tests/UndoEditor/UndoTests.cs.meta': '0fc663a1683d38ed2e763d136963bba9e1707dc9c34da601583a49a9dc2a01c8',
 'unity/Assets/Game/Tests/UndoPlay.meta': '9ec9aeddf66dc66e9051f7fa28d32a0ac6856dd3941eae59a5ed12fec42a95b8',
 'unity/Assets/Game/Tests/UndoPlay/Nard.Tests.UndoPlay.asmdef': 'f8142bdb1be1446f8485bc33b1abb5e458b878e56532bd8c5346a14ef6549922',
 'unity/Assets/Game/Tests/UndoPlay/Nard.Tests.UndoPlay.asmdef.meta': '5bb33966eee8655a23a4b134995148b24fa6e9565f15b875c87e8ec7e0855603',
 'unity/Assets/Game/Tests/UndoPlay/UndoSceneTests.cs': '6f89095069fafc93042caba9f4318c6e1d4d7c6275785c899bd3dc82a8f158dd',
 'unity/Assets/Game/Tests/UndoPlay/UndoSceneTests.cs.meta': '8dc049fc1ba9fd4eaac68cc6d4889fdbb8d124f43c78021ae7e7dc83977467d7'})
SOURCES = {
    'NARD_Final_GDD_v1.0.md': '2d2471282160bc10dc4eaec57ed90b8b2e6e7684cb4101c9bd5f2d76e90a8580',
    'NARD_Master_Icra_Plani_v2.md': '1e333827bf7c861c5fc2b279818c795c69ddeabbd1fa8c93e3b7daad2d6828bc',
}
REQUIRED = [
    '.gitignore', '.gitattributes', 'AGENTS.md', 'README.md', 'PROGRESS.md', 'CHANGELOG.md',
    'docs/STATE.json', 'docs/DECISIONS.md', 'docs/APPROVED.md', 'docs/REJECTED.md',
    'docs/ARCHITECTURE.md', 'docs/OPERATIONS.md', 'docs/TEST_MATRIX.md',
    'docs/ASSET_MANIFEST.csv', 'docs/reports/TEMPLATE.md', 'docs/reports/P00.1.md',
    'docs/reports/P00.2.md', 'scripts/validation/validate_project.py',
    'scripts/validation/test_foundation.py', 'docs/reports/P00.2.validation.json',
    'docs/reports/P00.2.tests.txt', 'docs/reports/P00.3.md',
    'docs/reports/P00.3.validation.json', 'docs/reports/P00.3.tests.txt',
    'unity/Packages/manifest.json', 'unity/Packages/packages-lock.json',
    'unity/ProjectSettings/ProjectVersion.txt', 'scripts/build/p003.py',
    'docs/reports/P00.3.execution.json', 'docs/screenshots/P00.3-editor.png',
    'docs/screenshots/P00.3-gameview.png',
    'docs/RULES_LONG.md', 'docs/RULES_SHORT.md', 'docs/RULESET_VARIANTS.md',
    'docs/RULE_MATRIX.md', 'docs/rules/fixtures.json', 'docs/rules/fixtures.schema.json',
    'scripts/validation/validate_rule_fixtures.py', 'scripts/validation/test_rule_fixtures.py',
    'docs/reports/P01.1.md', 'docs/reports/P01.1.validation.json',
    'docs/reports/P01.1.tests.txt', 'docs/reports/P01.1.rules.json',
    'docs/reports/P01.2.md', 'docs/reports/P01.2.execution.json',
    'docs/reports/P01.2.validation.json', 'docs/reports/P01.2.tests.txt',
    'scripts/build/p012.py', 'scripts/validation/test_p012_runner.py',
    'docs/reports/P01.2.editmode.xml', 'docs/reports/P01.2.playmode.xml', 'unity/Assets/Game/Rules/LongNardeRules.cs',
    'unity/Assets/Game/Rules/LongPosition.cs', 'unity/Assets/Game/Rules/Nard.Rules.Long.asmdef',
    'unity/Assets/Game/Tests/Rules/LongRulesTests.cs',
    'unity/Assets/Game/Tests/Rules/Nard.Rules.Long.Tests.asmdef',
    'unity/Assets/Game/Rules/Short/ShortPosition.cs', 'unity/Assets/Game/Rules/Short/ShortNardeRules.cs',
    'unity/Assets/Game/Rules/Short/Nard.Rules.Short.asmdef',
    'unity/Assets/Game/Tests/ShortRules/ShortRulesTests.cs',
    'unity/Assets/Game/Tests/ShortRules/Nard.Rules.Short.Tests.asmdef',
    'docs/reports/P01.3.md', 'docs/reports/P01.3.execution.json',
    'docs/reports/P01.3.validation.json', 'docs/reports/P01.3.tests.txt',
    'docs/reports/P01.3.editmode.xml', 'docs/reports/P01.3.playmode.xml',
    'unity/Assets/Game/Rules/Match/MatchContracts.cs', 'unity/Assets/Game/Rules/Match/MatchSession.cs',
    'unity/Assets/Game/Rules/Match/MatchSeries.cs', 'unity/Assets/Game/Rules/Match/Nard.Match.asmdef',
    'unity/Assets/Game/Tests/Match/MatchTests.cs', 'unity/Assets/Game/Tests/Match/Nard.Match.Tests.asmdef',
    'docs/reports/P01.4.md', 'docs/reports/P01.4.execution.json', 'docs/reports/P01.4.validation.json',
    'docs/reports/P01.4.tests.txt', 'docs/reports/P01.4.editmode.xml', 'docs/reports/P01.4.playmode.xml',
    'unity/Assets/Game/Presentation/BoardSnapshot.cs', 'unity/Assets/Game/Presentation/BoardGeometry.cs',
    'unity/Assets/Game/Presentation/BoardView.cs', 'unity/Assets/Game/Scenes/TechnicalBoard.unity',
    'unity/Assets/Game/Editor/P021Build.cs', 'unity/Assets/Game/Tests/BoardEditor/BoardProjectionTests.cs',
    'unity/Assets/Game/Tests/BoardPlayMode/BoardSceneTests.cs', 'docs/reports/P02.1.md',
    'docs/reports/P02.1.execution.json', 'docs/reports/P02.1.validation.json', 'docs/reports/P02.1.tests.txt',
    'docs/reports/P02.1.editmode.xml', 'docs/reports/P02.1.playmode.xml',
    'unity/Assets/Game/Interaction/TapSelection.cs', 'unity/Assets/Game/Interaction/Nard.Interaction.asmdef',
    'unity/Assets/Game/Presentation/BoardTapInput.cs', 'unity/Assets/Game/Presentation/BoardHitMap.cs',
    'unity/Assets/Game/Scenes/TapBoard.unity', 'unity/Assets/Game/Editor/P022Build.cs',
    'unity/Assets/Game/Tests/TapEditor/TapSelectionTests.cs', 'unity/Assets/Game/Tests/TapPlayMode/TapSceneTests.cs',
    'docs/reports/P02.2.md', 'docs/reports/P02.2.execution.json', 'docs/reports/P02.2.tests.txt',
    'docs/reports/P02.2.validation.json', 'docs/reports/P02.2.editmode.xml', 'docs/reports/P02.2.playmode.xml',
    'unity/Assets/Game/Interaction/LocalDiceSource.cs', 'unity/Assets/Game/Interaction/LocalTurnLoop.cs',
    'unity/Assets/Game/Presentation/BoardTurnInput.cs', 'unity/Assets/Game/Scenes/TurnBoard.unity',
    'unity/Assets/Game/Editor/P023Build.cs', 'unity/Assets/Game/Editor/P023Preview.cs',
    'unity/Assets/Game/Tests/TurnEditor/LocalTurnTests.cs', 'unity/Assets/Game/Tests/TurnPlayMode/TurnSceneTests.cs',
    'docs/reports/P02.3.md', 'docs/reports/P02.3.execution.json', 'docs/reports/P02.3.tests.txt',
    'docs/reports/P02.3.validation.json', 'docs/reports/P02.3.editmode.xml', 'docs/reports/P02.3.playmode.xml',
]
REQUIRED += ['docs/reports/P02.4.md', 'docs/reports/P02.4.execution.json', 'docs/reports/P02.4.tests.txt',
 'docs/reports/P02.4.validation.json', 'docs/reports/P02.4.editmode.xml', 'docs/reports/P02.4.playmode.xml',
 'unity/Assets/Game/Interaction/UndoPreview.cs', 'unity/Assets/Game/Scenes/UndoBoard.unity',
 'unity/Assets/Game/Editor/P024Build.cs', 'unity/Assets/Game/Editor/P024Preview.cs',
 'unity/Assets/Game/Tests/UndoEditor/UndoTests.cs', 'unity/Assets/Game/Tests/UndoEditor/UndoRegressionTests.cs',
 'unity/Assets/Game/Tests/UndoPlay/UndoSceneTests.cs']
REQUIRED += ['docs/reports/P02.5.md', 'docs/reports/P02.5.execution.json', 'docs/reports/P02.5.validation.json',
 'docs/reports/P02.5.tests.txt', 'docs/reports/P02.5.editmode.xml', 'docs/reports/P02.5.playmode.xml',
 'unity/Assets/Game/Presentation/MatchBoardLayout.cs', 'unity/Assets/Game/Presentation/MatchHudView.cs',
 'unity/Assets/Game/Scenes/LayoutBoard.unity', 'unity/Assets/Game/Editor/P025Build.cs',
 'unity/Assets/Game/Editor/P025Preview.cs', 'unity/Assets/Game/Tests/LayoutEditor/MatchLayoutTests.cs',
 'unity/Assets/Game/Tests/LayoutPlay/MatchLayoutSceneTests.cs']
ASSET_COLUMNS = ['asset_id', 'path', 'source', 'creator', 'license', 'license_evidence',
                 'modifications', 'approved_part', 'status']
SECRET_PATTERNS = [
    re.compile(rb'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----'),
    re.compile(rb'\bgh[pousr]_[A-Za-z0-9]{30,}\b'),
    re.compile(rb'\bgithub_pat_[A-Za-z0-9_]{30,}\b'),
    re.compile(rb'\bAKIA[A-Z0-9]{16}\b'),
]
IGNORE_PROBES = [
    'Library/cache.bin', 'unity/Library/cache.bin', 'unity/Temp/cache.bin',
    'unity/Obj/cache.bin', 'unity/Build/a.apk', 'unity/Builds/a.aab',
    'unity/Logs/editor.log', 'unity/UserSettings/layout.wlt', 'node_modules/a/index.js',
    '.env', '.env.production', 'nakama/.env', 'signing.p12', 'release.keystore',
    'signing.key', 'credentials.json', 'service-account-test.json',
    '.codex-gravity/private.json', 'scripts/__pycache__/file.pyc',
    'unity/.utmp/Debug/object.o', 'unity/.gradle/cache.bin', 'unity/.cxx/cache.bin',
    'unity/Assets/_Recovery/0.unity', 'unity/Assets/_Recovery.meta',
]
KEEP_PROBES = [
    'AGENTS.md', 'docs/STATE.json', '.env.example', 'unity/Assets/Game/Rules/Long.cs',
    'unity/Assets/Game/Art/board.png', 'unity/Assets/Game/Scenes/Main.unity',
    'unity/Assets/Game/Scenes/Main.unity.meta', 'unity/Packages/manifest.json',
    'unity/ProjectSettings/ProjectVersion.txt', 'nakama/server/main.ts',
    'docs/reports/P00.2.validation.json', 'scripts/build/build_android.py',
]


def git(root, *args, input_text=None):
    return subprocess.run(['git', '-C', str(root), *args], input=input_text,
                          text=True, capture_output=True, timeout=20)


def require(condition, message):
    if not condition:
        raise ValueError(message)


def read(root, name):
    return (root / name).read_text(encoding='utf-8')


def state(root):
    return json.loads(read(root, 'docs/STATE.json'))


def parts(root):
    return re.findall(r'^\*\*(P\d{2}\.\d+) —', read(root, 'NARD_Master_Icra_Plani_v2.md'), re.M)


def inventory(root, prefix):
    return re.findall(r'^\d+\. \*\*(' + prefix + r'\d{2})\*\* (.+)$',
                      read(root, 'NARD_Final_GDD_v1.0.md'), re.M)


def validate(root):
    root = Path(root).resolve()
    checks = []

    def check(name, action):
        try:
            detail = action() or 'OK'
            checks.append({'name': name, 'status': 'PASS', 'detail': detail})
        except (ValueError, KeyError, TypeError, IndexError, AttributeError, OSError, ET.ParseError, subprocess.SubprocessError) as exc:
            checks.append({'name': name, 'status': 'FAIL', 'detail': str(exc)})

    def required():
        missing = [n for n in [*SOURCES, *REQUIRED] if not (root / n).is_file()]
        require(not missing, 'Missing: ' + ', '.join(missing))
        return f'{len(SOURCES) + len(REQUIRED)} required files exist'
    check('required_files', required)

    def source_integrity():
        for name, expected in SOURCES.items():
            require(hashlib.sha256((root / name).read_bytes()).hexdigest() == expected,
                    f'Source changed: {name}')
            require(state(root)['sources'][name]['sha256'] == expected, f'Hash ledger mismatch: {name}')
        return 'Both original source files and recorded SHA-256 match baseline'
    check('source_integrity', source_integrity)

    def decisions():
        for prefix, count, file, status, width in [
            ('A', 87, 'docs/APPROVED.md', 'NOT_STARTED', 4),
            ('R', 20, 'docs/REJECTED.md', 'DO_NOT_BUILD', 3),
        ]:
            expected = inventory(root, prefix)
            require([i for i, _ in expected] == [f'{prefix}{i:02}' for i in range(1, count + 1)],
                    f'{prefix} source IDs invalid')
            rows = [[c.strip() for c in line.strip().strip('|').split('|')]
                    for line in read(root, file).splitlines()
                    if re.match(r'^\| ' + prefix + r'\d{2} \|', line)]
            require(len(rows) == count and all(len(row) == width for row in rows), f'{file} row count/shape')
            require([(r[0], r[1]) for r in rows] == expected, f'{file} differs from GDD wording/order')
            require(all(r[2] == ('IN_PROGRESS' if prefix == 'A' and r[0] in ['A01', 'A02', 'A18', 'A44', 'A48', 'A47', 'A49', 'A50', 'A51', 'A53', 'A54'] else status) for r in rows), f'{file} implementation status invalid')
            if prefix == 'A':
                require(all(r[3] in parts(root) for r in rows), 'Unknown planned part in APPROVED')
            master = re.findall(r'^\d+\. \*\*(' + prefix + r'\d{2})\*\* (.+)$',
                                read(root, 'NARD_Master_Icra_Plani_v2.md'), re.M)
            require(master == expected, f'{prefix} GDD/Master disagreement')
        return '87 approved + 20 rejected: exact text, order, status and part links'
    check('decision_inventory', decisions)

    def state_contract():
        s = state(root)
        require(s['schema_version'] == 1, 'Unsupported state schema')
        require(s['timezone'] == 'Asia/Baku', 'Timezone mismatch')
        datetime.strptime(s['updated_on'], '%Y-%m-%d')
        known = parts(root)
        require(s['active_part'] in known, 'Unknown active part')
        active = s['active_part']
        require(s['authorization']['part'] == active and s['authorization']['by'] == 'human'
                and bool(s['authorization']['quote'].strip()), 'Missing human scope authorization')
        require(s['approved_feature_count'] == 87 and s['rejected_feature_count'] == 20, 'Feature count mismatch')
        for flag in ['current_part_human_approved', 'next_part_authorized']:
            require(type(s[flag]) is bool, f'{flag} must be boolean')
        approvals = s['human_approvals']
        approved_parts = [a['part'] for a in approvals]
        require(len(approved_parts) == len(set(approved_parts)), 'Duplicate human approval')
        for a in approvals:
            require(a['part'] in known and a['by'] == 'human' and a['quote'].strip(), 'Invalid approval record')
            require(known.index(a['part']) < known.index(active), 'Current/future part falsely human-approved')
            require((root / a['report']).is_file(), 'Approval report missing')
        require(bool(approvals) and approved_parts[-1] == s['last_human_approved_part'], 'Approval head mismatch')
        require(s['next_part'] == known[known.index(active) + 1], 'Next part out of plan order')
        require(s['status'] in ['PLANNED', 'IN_PROGRESS', 'SELF_TEST_FAILED', 'SELF_TEST_PASSED',
                               'AWAITING_HUMAN_APPROVAL', 'REWORK_REQUIRED', 'HUMAN_APPROVED'], 'Unknown status')
        if s['status'] == 'AWAITING_HUMAN_APPROVAL':
            require(not s['current_part_human_approved'] and not s['next_part_authorized'], 'Approval gate bypass')
        require(set(s['checks']) == {'validation', 'validator_tests', 'unity_build', 'visual_device',
                                     'validation_evidence', 'tests_evidence'}, 'Incomplete check ledger')
        for name, value in s['checks'].items():
            if not name.endswith('_evidence'):
                require(value in ['PASS', 'FAIL', 'BLOCKED', 'NOT_RUN'], f'Invalid check status: {name}')
        require((root / s['test_report']).is_file(), 'Current report missing')
        for item in s['open_items']:
            require(all(item[k] for k in ['id', 'status', 'impact', 'next_action']), 'Incomplete open item')
        require(len({i['id'] for i in s['open_items']}) == len(s['open_items']), 'Duplicate open item ID')
        return f'{active}: authorization and approval gate consistent'
    check('state_contract', state_contract)

    def evidence():
        s = state(root)
        paths = {}
        for key in ['validation_evidence', 'tests_evidence']:
            target = (root / s['checks'][key]).resolve()
            require(target.is_relative_to(root) and target.is_file(), f'Missing/unsafe evidence: {key}')
            paths[key] = target
        if s['checks']['validation'] == 'PASS':
            report = json.loads(paths['validation_evidence'].read_text(encoding='utf-8'))
            checks = report['checks']
            require(report['part'] == s['active_part'] and bool(checks), 'Evidence part/check list mismatch')
            require(report['failed'] == 0 and report['passed'] == len(checks)
                    and all(c['status'] == 'PASS' for c in checks), 'Validation PASS lacks passing evidence')
        if s['checks']['validator_tests'] == 'PASS':
            log = paths['tests_evidence'].read_text(encoding='utf-8')
            require(re.search(r'^Ran [1-9][0-9]* tests? in ', log, re.M) is not None
                    and '\nOK\n' in log and log.rstrip().endswith('Exit code: 0'),
                    'Test PASS lacks successful run evidence')
        if s.get('engine', {}).get('status') == 'PASS':
            import xml.etree.ElementTree as ET
            engine_path = (root / s['engine']['execution_evidence']).resolve()
            require(engine_path.is_relative_to(root), 'Unsafe engine evidence path')
            execution = json.loads(engine_path.read_text())
            require(execution['part'] == AUTHORIZED_PART and execution['physical_android'] == 'NOT_RUN'
                    and execution['independent_local_player_review'] == 'NOT_RUN', 'Engine evidence scope mismatch')
            for step in ['editmode', 'playmode', 'android']:
                report = execution['steps'][step]
                require(report['status'] == 'PASS' and report['exit_code'] == 0 and not report['compiler_errors'], 'Engine/build run failed')
                for name, digest in report['source_sha256'].items():
                    if name.startswith('unity/') or name == 'docs/rules/fixtures.json':
                        path = (root / name).resolve()
                        require(path.is_relative_to(root) and hashlib.sha256(path.read_bytes()).hexdigest() == digest,
                                'Engine evidence source mismatch: ' + name)
                if step != 'android':
                    document = ET.parse(root / ('docs/reports/P02.5.' + step + '.xml')).getroot()
                    require(document.attrib['failed'] == '0' and int(document.attrib['passed']) > 0, 'Failed NUnit XML')
                    require(document.attrib['passed'] == report['tests']['passed'] and document.attrib['skipped'] == report['tests']['skipped'], 'NUnit summary mismatch')
            require(execution['coverage']['executed_long_fixtures'] == 26 and execution['coverage']['executed_short_fixtures'] == 30
                    and execution['coverage']['executed_match_fixtures'] == 7
                    and execution['coverage']['deferred_unique_fixtures'] == 0,
                    'Fixture execution coverage mismatch')
            require(execution['steps']['editmode']['executed_board_cases'] == 38 and execution['steps']['playmode']['executed_board_cases'] == 1, 'Board suites not executed')
            require(execution['steps']['editmode']['executed_tap_cases'] == 55 and execution['steps']['playmode']['executed_tap_cases'] == 5 and execution['steps']['editmode']['executed_tap_fixtures'] == 43, 'Tap suites not executed')
            require(execution['steps']['editmode']['executed_turn_cases'] == 168 and execution['steps']['playmode']['executed_turn_cases'] == 5 and execution['steps']['editmode']['executed_turn_roll_cases'] == 144, 'Turn suites not executed')
            require(execution['steps']['editmode']['executed_undo_fixtures'] == 4 and execution['steps']['editmode']['executed_undo_tests'] == 194 and execution['steps']['playmode']['executed_undo_tests'] == 5, 'Undo suites not executed')
            require(execution['steps']['editmode']['executed_reference_skin_cases'] == 27, 'Reference skin input mapping not tested')
            require(execution['android_signature']['status'] == 'PASS' and execution['android_signature']['apk_sha256'] == execution['steps']['android']['apk_sha256'], 'APK signature evidence mismatch')
            require(s['build']['apk_sha256'] == execution['steps']['android']['apk_sha256'] and s['build']['apk_bytes'] == execution['steps']['android']['apk_bytes'], 'Build ledger mismatch')
            require(execution['steps']['editmode']['executed_layout_cases'] == 40 and execution['steps']['playmode']['executed_layout_cases'] == 10, 'Responsive suites not executed')
            visual = execution['visual']
            require(visual['status'] == 'PASS' and visual['physical_device'] == 'NOT_RUN', 'Visual/device evidence mixed')
            require({row['kind'] for row in visual['images']} == {'layout-landscape', 'layout-portrait', 'layout-wide-safe', 'layout-tall-safe', 'layout-tablet', 'layout-portrait-moving', 'layout-short-bar', 'layout-short-off', 'editor-gameview'}, 'Missing board screenshot')
            require(len(visual.get('additional_images', [])) == 2 and len(visual.get('before_images', [])) == 2, 'D034 comparison/selection renders missing')
            require(len(visual.get('portrait_phone_images', [])) == 5, 'D035 portrait phone/stack evidence missing')
            comparison = visual.get('landscape_pixel_comparison', [])
            require(len(comparison) == 4 and all(row['changed_pixels'] <= 1 and row.get('max_channel_delta', 0) <= 1 and row['baseline_commit'] == '0b6db69' for row in comparison), 'Landscape pixel regression changed')
            require(len(visual.get('portrait_stack_images', [])) == 14, 'D036 stack screenshots missing')
            measured = execution.get('portrait_measurements', {}).get('rows', [])
            expected = {(w, h, n) for w, h in [(320,568),(390,844),(540,960),(720,1600)] for n in [1,2,5,10,15]}
            require(len(measured) == 20 and {(r['width'],r['height'],r['count']) for r in measured} == expected, 'D036 measured size/count matrix incomplete')
            require(all(r['min_renderer_clearance'] > 0 and abs(r['sprite_footprint']/r['lane_width']-.80) < .001 for r in measured), 'D036 renderer touches cell or wrong lane ratio')
            for row in visual['images'] + visual['additional_images'] + visual['before_images'] + visual['portrait_phone_images'] + visual['portrait_stack_images']:
                path = (root / row['path']).resolve()
                require(path.is_relative_to(root) and path.is_file() and hashlib.sha256(path.read_bytes()).hexdigest() == row['sha256'], 'Screenshot missing or changed')
            corpus = json.loads(read(root, 'docs/rules/fixtures.json'))['cases']
            ledger = execution['fixture_execution']
            require([r['id'] for r in ledger] == [c['id'] for c in corpus], 'Fixture ledger IDs/order mismatch')
            xml = ET.parse(root / 'docs/reports/P02.5.editmode.xml').getroot()
            passed_names = {c.attrib['name'] for c in xml.iter('test-case') if c.attrib['result'] == 'Passed'}
            for case, row in zip(corpus, ledger):
                if case['action']['kind'] == 'undo':
                    name = 'UndoFixture_' + case['id']
                    require(row['status'] == 'PASS' and row['test_name'] == name and name in passed_names, 'Undo PASS lacks matching NUnit case')
                else:
                    prefix = 'MatchFixture_' if case['action']['kind'] in ['series', 'rating_policy'] else 'Fixture_' if case['mode'] == 'long' else 'ShortFixture_'
                    name = prefix + case['id']
                    require(row['status'] == 'PASS' and row['test_name'] == name and name in passed_names, 'Fixture PASS lacks matching NUnit case')

        return 'Stored PASS evidence and runtime source hashes match; independent device/local reviews remain NOT_RUN'
    check('test_evidence', evidence)

    def scope():
        s = state(root)
        # Updated only after the human's explicit P02.5 authorization (D033).
        require(s['active_part'] == AUTHORIZED_PART, 'Only P02.5 is authorized')
        require(s['status'] == 'AWAITING_HUMAN_APPROVAL', 'P02.5 must await human approval')
        require(s['last_human_approved_part'] == 'P02.4', 'P02.4 acceptance missing')
        expected = {'unity_project_created', 'android_tools_installed', 'game_modules_implemented'}
        require(set(s['scope']) == expected, 'Scope fields missing or unexpected')
        require(all(type(value) is bool for value in s['scope'].values()), 'Scope flags must be booleans')
        require(s['scope']['unity_project_created'] is True and s['scope']['game_modules_implemented'] is True,
                'P02.5 permits only responsive match layouts on frozen engines')
        require(not any((root / p).exists() for p in ['nakama', 'art']), 'Premature game/backend directories')
        for module in ['Bots', 'Network', 'UI']:
            require(not (root / 'unity/Assets/Game' / module).exists(), 'Premature gameplay module: ' + module)
        version = read(root, 'unity/ProjectSettings/ProjectVersion.txt')
        require('m_EditorVersion: 6000.6.7f1\n' in version, 'Unity version not pinned')
        manifest = json.loads(read(root, 'unity/Packages/manifest.json'))['dependencies']
        require(manifest['com.unity.test-framework'] == '1.8.0', 'Test Framework version mismatch')
        require(manifest['com.unity.inputsystem'] == '1.20.1', 'Input system version mismatch')
        locked = json.loads(read(root, 'unity/Packages/packages-lock.json'))['dependencies']
        for name in ['com.unity.inputsystem', 'com.unity.test-framework']:
            require(locked[name]['version'] == manifest[name], 'Manifest/lock mismatch: ' + name)
        require((root / 'unity/Assets/Game/Scenes/Bootstrap.unity').is_file(), 'Bootstrap scene missing')
        assembly = json.loads(read(root, 'unity/Assets/Game/Rules/Nard.Rules.Long.asmdef'))
        require(assembly.get('noEngineReferences') is True and not assembly.get('references'), 'Rules assembly must be pure C#')
        files = sorted(p.name for p in (root / 'unity/Assets/Game/Rules').glob('*.cs'))
        require(files == ['LongNardeRules.cs', 'LongPosition.cs'], 'Only long-engine sources authorized')
        for file in files:
            code = read(root, 'unity/Assets/Game/Rules/' + file)
            require(not re.search(r'UnityEngine|UnityEditor|System\.Random|System\.IO|DateTime|Nakama', code), 'Rules dependency violation')
        short_assembly = json.loads(read(root, 'unity/Assets/Game/Rules/Short/Nard.Rules.Short.asmdef'))
        require(short_assembly.get('noEngineReferences') is True and not short_assembly.get('references'), 'Short assembly must be independent pure C#')
        expected_sources = {'LongNardeRules.cs', 'LongPosition.cs', 'Short/ShortNardeRules.cs', 'Short/ShortPosition.cs', 'Match/MatchContracts.cs', 'Match/MatchSession.cs', 'Match/MatchSeries.cs'}
        actual_sources = {p.relative_to(root / 'unity/Assets/Game/Rules').as_posix() for p in (root / 'unity/Assets/Game/Rules').rglob('*.cs')}
        require(actual_sources == expected_sources, 'Unauthorized rules sources')
        for file in ['ShortNardeRules.cs', 'ShortPosition.cs']:
            code = read(root, 'unity/Assets/Game/Rules/Short/' + file)
            require(not re.search(r'UnityEngine|UnityEditor|System\.Random|System\.IO|DateTime|Nakama|Nard\.Rules\.Long', code), 'Short dependency violation')
        for name, digest in ENGINE_BASELINE.items():
            require(hashlib.sha256((root / name).read_bytes()).hexdigest() == digest, 'Accepted engine code/test changed: ' + name)
        match_assembly = json.loads(read(root, 'unity/Assets/Game/Rules/Match/Nard.Match.asmdef'))
        require(match_assembly.get('noEngineReferences') is True and
                match_assembly.get('references') == ['Nard.Rules.Long', 'Nard.Rules.Short'], 'Match dependency boundary violated')
        for file in ['MatchContracts.cs', 'MatchSession.cs', 'MatchSeries.cs']:
            code = read(root, 'unity/Assets/Game/Rules/Match/' + file)
            require(not re.search(r'UnityEngine|UnityEditor|System\.Random|System\.IO|DateTime|Nakama', code), 'Match dependency violation')
        presentation = root / 'unity/Assets/Game/Presentation'
        require({p.name for p in presentation.glob('*.cs')} == {'BoardSnapshot.cs', 'BoardGeometry.cs', 'BoardView.cs', 'BoardHitMap.cs', 'BoardTapInput.cs', 'BoardTurnInput.cs', 'ReferenceBoardSkin.cs', 'MatchBoardLayout.cs', 'MatchHudView.cs'}, 'Unauthorized presentation sources')
        for file in presentation.glob('*.cs'):
            code = file.read_text()
            require(not re.search(r'TryApply|LegalTurns|SupplyRoll|AdvanceTurn|NextDie\(|RandomNumberGenerator|Random\.|OnMouse|IPointer|Nakama', code), 'Presentation duplicates rules or future interaction')
        assembly = json.loads(read(root, 'unity/Assets/Game/Presentation/Nard.Presentation.asmdef'))
        require(assembly['references'] == ['Nard.Match', 'Nard.Rules.Long', 'Nard.Rules.Short', 'Nard.Interaction', 'Unity.InputSystem'], 'Presentation dependency violation')
        require('Müvəqqəti qrafika' in read(root, 'unity/Assets/Game/Presentation/BoardView.cs'), 'Temporary graphic disclosure missing')
        interaction = json.loads(read(root, 'unity/Assets/Game/Interaction/Nard.Interaction.asmdef'))
        require(interaction.get('noEngineReferences') is True and interaction['references'] == ['Nard.Match', 'Nard.Rules.Long', 'Nard.Rules.Short'], 'Tap application must be pure C#')
        code = read(root, 'unity/Assets/Game/Interaction/TapSelection.cs')
        require(not re.search(r'UnityEngine|UnityEditor|System\.Random|SupplyRoll|AdvanceTurn|TryCommitTurn|System\.IO|DateTime|Nakama', code), 'Tap application exceeds P02.2 scope')
        require({p.name for p in (root / 'unity/Assets/Game/Interaction').glob('*.cs')} == {'TapSelection.cs', 'LocalTurnLoop.cs', 'LocalDiceSource.cs', 'UndoPreview.cs'}, 'Unauthorized application sources')
        code = read(root, 'unity/Assets/Game/Interaction/LocalTurnLoop.cs')
        require(not re.search(r'UnityEngine|UnityEditor|RandomNumberGenerator|System\.Random|System\.IO|DateTime|Nakama', code), 'Turn loop dependency violation')
        source = read(root, 'unity/Assets/Game/Interaction/LocalDiceSource.cs')
        require('RandomNumberGenerator.GetInt32(1,7)' in source and not re.search(r'UnityEngine|UnityEditor|Nakama', source), 'Dice source must remain separate and local')
        code = read(root, 'unity/Assets/Game/Interaction/UndoPreview.cs')
        require(not re.search(r'UnityEngine|UnityEditor|NextDie|SupplyRoll|AdvanceTurn|TryCommitTurn|System\.IO|DateTime|Nakama', code), 'Undo preview boundary violated')
        review = s['rules_review']
        require(review['preliminary_standard'] == 'ACCEPTED_BY_HUMAN' and
                review['independent_azerbaijani_players'] == 'NOT_RUN' and
                review['provisional_rules_retained'] is True, 'Local review falsely completed')
        return 'P02.5: responsive match layouts; accepted engines/match/tap/tests frozen; local review NOT_RUN'
    check('current_scope', scope)

    def policy():
        p = state(root)['tool_policy']
        for name in ['skills_allowed', 'antigravity_related_mcp_allowed', 'codex_gravity_mcp_allowed']:
            require(p[name] is False, f'Tool ban missing: {name}')
        require(p['standard_terminal_git_allowed'] is True, 'Standard tools must remain allowed')
        a = read(root, 'AGENTS.md')
        for term in ['Heç bir Skill', 'Antigravity MCP', 'Codex Gravity MCP', 'AWAITING_HUMAN_APPROVAL',
                     'P02.5', 'R01–R20']:
            require(term in a, f'Agent rule missing: {term}')
        return 'Agent-neutral rules and tool bans present'
    check('agent_policy', policy)

    def progress():
        s = state(root)
        expected = {'Aktiv part': s['active_part'], 'Status': s['status'],
                    'Son insan təsdiqi': s['last_human_approved_part'],
                    'Cari partın insan təsdiqi': str(s['current_part_human_approved']).lower(),
                    'Növbəti part': s['next_part'],
                    'Növbəti parta icazə': str(s['next_part_authorized']).lower()}
        text = read(root, 'PROGRESS.md')
        for label, value in expected.items():
            require(f'- {label}: `{value}`' in text, f'PROGRESS mismatch: {label}')
        return 'Human progress view matches STATE'
    check('progress_consistency', progress)

    def links():
        count = 0
        names = git(root, 'ls-files', '--cached', '--others', '--exclude-standard', '-z')
        require(names.returncode == 0, 'Cannot enumerate documentation')
        for name in sorted(set(filter(None, names.stdout.split('\0')))):
            file = root / name
            if file.suffix != '.md' or file.name in SOURCES:
                continue  # Immutable originals contain illustrative future paths.
            text = re.sub(r'```.*?```', '', file.read_text(encoding='utf-8'), flags=re.S)
            for target in re.findall(r'\[[^\]]*\]\(([^\s)]+)\)', text):
                target = target.strip('<>')
                if urlsplit(target).scheme or target.startswith('#'):
                    continue
                path = unquote(target.split('#', 1)[0])
                dest = (file.parent / path).resolve()
                require(dest.is_relative_to(root), f'Link escapes repo: {file.relative_to(root)}')
                require(dest.exists(), f'Broken link: {file.relative_to(root)} -> {target}')
                count += 1
        return f'{count} local Markdown links resolve'
    check('local_links', links)

    def assets():
        with (root / 'docs/ASSET_MANIFEST.csv').open(encoding='utf-8', newline='') as f:
            reader = csv.DictReader(f)
            require(reader.fieldnames == ASSET_COLUMNS, 'Asset schema mismatch')
            rows = list(reader)
        seen = set()
        for row in rows:
            require(set(row) == set(ASSET_COLUMNS), 'Malformed CSV asset row')
            require(row['asset_id'] and row['asset_id'] not in seen, 'Duplicate/empty asset ID')
            seen.add(row['asset_id'])
            require(row['source'] and row['creator'] and row['license'] and row['license_evidence'], 'Missing asset rights')
            p = (root / row['path']).resolve()
            require(bool(row['path']) and p.is_relative_to(root) and p.is_file(), 'Invalid asset path')
        return f'{len(rows)} assets registered'
    check('asset_manifest', assets)

    def git_root():
        r = git(root, 'rev-parse', '--show-toplevel')
        require(r.returncode == 0 and Path(r.stdout.strip()).resolve() == root, 'Wrong/missing Git root')
        branch = git(root, 'symbolic-ref', '--short', 'HEAD')
        require(branch.returncode == 0 and branch.stdout.strip() == state(root)['repository']['branch'], 'Branch mismatch')
        return 'Git root and branch match STATE'
    check('git_root', git_root)

    def ignores():
        for path in IGNORE_PROBES:
            r = git(root, 'check-ignore', '--no-index', '-q', '--', path)
            require(r.returncode == 0, f'Sensitive/cache path not ignored: {path}')
        for path in KEEP_PROBES:
            r = git(root, 'check-ignore', '--no-index', '-q', '--', path)
            require(r.returncode == 1, f'Important source incorrectly ignored: {path}')
        return f'{len(IGNORE_PROBES)} excluded + {len(KEEP_PROBES)} retained paths checked'
    check('gitignore_behavior', ignores)

    def file_safety():
        listing = git(root, 'ls-files', '--cached', '--others', '--exclude-standard', '-z')
        require(listing.returncode == 0, 'Cannot enumerate candidate files')
        paths = sorted(set(filter(None, listing.stdout.split('\0'))))
        tracked = git(root, 'ls-files', '-z')
        require(tracked.returncode == 0, 'Cannot enumerate index')
        for name in filter(None, tracked.stdout.split('\0')):
            require(git(root, 'check-ignore', '--no-index', '-q', '--', name).returncode == 1,
                    f'Ignored file already tracked: {name}')
        for name in paths:
            p = root / name
            require(not p.is_symlink(), f'Symlink needs explicit review: {name}')
            require(p.resolve().is_relative_to(root), f'Path escapes repo: {name}')
            data = p.read_bytes()
            require(allowed_file_size(name, data), f'File size/content needs review: {name}')
            require(not any(pattern.search(data) for pattern in SECRET_PATTERNS), f'Secret-like content: {name}')
            blob = subprocess.run(['git', '-C', str(root), 'show', ':' + name], capture_output=True, timeout=20)
            if blob.returncode == 0:
                require(allowed_file_size(name, blob.stdout), f'Staged file size/content needs review: {name}')
                require(not any(pat.search(blob.stdout) for pat in SECRET_PATTERNS), f'Staged secret-like content: {name}')
        return f'{len(paths)} candidate files and available index blobs checked; limited pattern scan'
    check('candidate_and_index_safety', file_safety)

    def remote_safety():
        repo = state(root)['repository']
        r = git(root, 'remote', 'get-url', 'origin')
        require(r.returncode == 0 and r.stdout.strip() == repo['remote_url'], 'Remote ledger mismatch')
        require(repo['push_authorized'] is False, 'Current human instruction forbids push')
        require(repo['sync_status'] in ['NOT_PUSHED', 'DEFERRED_PUBLIC_REMOTE'],
                'Push recorded despite explicit no-push instruction')
        return f"Remote {repo['remote_visibility']}; sync {repo['sync_status']}"
    check('remote_safety', remote_safety)

    def rule_specification():
        from validate_rule_fixtures import validate as validate_rules
        result = validate_rules(root)
        require(state(root)['build']['physical_device'] == 'NOT_RUN', 'Physical device test must remain open')
        return f"{result['fixture_count']} authored examples; engine results separately in P02.5.execution.json, independent local review NOT_RUN"
    check('rule_specification', rule_specification)

    return {'schema_version': 1, 'part': state(root)['active_part'],
            'executed_at': datetime.now(ZoneInfo('Asia/Baku')).isoformat(timespec='seconds'),
            'passed': sum(c['status'] == 'PASS' for c in checks),
            'failed': sum(c['status'] == 'FAIL' for c in checks), 'checks': checks}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, default=ROOT)
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    try:
        report = validate(args.root)
    except (ValueError, KeyError, TypeError, OSError) as exc:
        report = {'passed': 0, 'failed': 1, 'checks': [{'name': 'read_state', 'status': 'FAIL', 'detail': str(exc)}]}
    output = json.dumps(report, ensure_ascii=False, indent=2) + '\n'
    if args.output:
        # The caller must choose an existing parent; this is only an evidence file.
        destination = args.output if args.output.is_absolute() else args.root / args.output
        destination.write_text(output, encoding='utf-8')
    print(output, end='')
    return int(report['failed'] != 0)


if __name__ == '__main__':
    raise SystemExit(main())
