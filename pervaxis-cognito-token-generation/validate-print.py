# Copyright (C) 2026 Clarivex Technologies Private Limited. All Rights Reserved.
"""Run handler contract tests from the templates without AWS or external packages.

Engine tests separately verify Scriban rendering and ZIP output. This fixture
substitutes only the public settings used by these templates, then runs the
generated tests against their InlineCode block.
"""
from pathlib import Path
import subprocess
import sys
import tempfile


def main():
    templates = Path(__file__).resolve().parent / "Templates"
    settings = {
        "{{ model.token_region }}": "us-east-1",
        "{{ model.token_user_pool_id }}": "us-east-1_Example",
        "{{ model.manifest.service_name }}": "identity-token-enrichment",
    }
    with tempfile.TemporaryDirectory(prefix="forge-token-tests-") as output:
        root = Path(output)
        for template in templates.rglob("*.sbn"):
            target = root / str(template.relative_to(templates))[:-4]
            target.parent.mkdir(parents=True, exist_ok=True)
            content = template.read_text(encoding="utf-8")
            for key, value in settings.items():
                content = content.replace(key, value)
            if "{{" in content:
                raise ValueError(f"Unhandled template expression: {template.name}")
            target.write_text(content, encoding="utf-8")
        return subprocess.call([sys.executable, "-m", "unittest", "discover", "-s", str(root / "tests"), "-v"])


if __name__ == "__main__":
    sys.exit(main())
