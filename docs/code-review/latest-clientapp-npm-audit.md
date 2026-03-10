# ClientApp npm audit

- Generated: 2026-03-10 19:16:40
- Exit code: 1
- JSON report: docs/code-review/latest-clientapp-npm-audit.json

## Raw output
```json
{
  "auditReportVersion": 2,
  "vulnerabilities": {
    "minimatch": {
      "name": "minimatch",
      "severity": "high",
      "isDirect": false,
      "via": [
        {
          "source": 1113465,
          "name": "minimatch",
          "dependency": "minimatch",
          "title": "minimatch has a ReDoS via repeated wildcards with non-matching literal in pattern",
          "url": "https://github.com/advisories/GHSA-3ppc-4f35-3m26",
          "severity": "high",
          "cwe": [
            "CWE-1333"
          ],
          "cvss": {
            "score": 0,
            "vectorString": null
          },
          "range": ">=9.0.0 <9.0.6"
        },
        {
          "source": 1113544,
          "name": "minimatch",
          "dependency": "minimatch",
          "title": "minimatch has ReDoS: matchOne() combinatorial backtracking via multiple non-adjacent GLOBSTAR segments",
          "url": "https://github.com/advisories/GHSA-7r86-cg39-jmmj",
          "severity": "high",
          "cwe": [
            "CWE-407"
          ],
          "cvss": {
            "score": 7.5,
            "vectorString": "CVSS:3.1/AV:N/AC:L/PR:N/UI:N/S:U/C:N/I:N/A:H"
          },
          "range": ">=9.0.0 <9.0.7"
        },
        {
          "source": 1113552,
          "name": "minimatch",
          "dependency": "minimatch",
          "title": "minimatch ReDoS: nested *() extglobs generate catastrophically backtracking regular expressions",
          "url": "https://github.com/advisories/GHSA-23c5-xmqv-rm74",
          "severity": "high",
          "cwe": [
            "CWE-1333"
          ],
          "cvss": {
            "score": 7.5,
            "vectorString": "CVSS:3.1/AV:N/AC:L/PR:N/UI:N/S:U/C:N/I:N/A:H"
          },
          "range": ">=9.0.0 <9.0.7"
        }
      ],
      "effects": [],
      "range": "9.0.0 - 9.0.6",
      "nodes": [
        "node_modules/minimatch"
      ],
      "fixAvailable": true
    },
    "moment": {
      "name": "moment",
      "severity": "high",
      "isDirect": true,
      "via": [
        {
          "source": 1091723,
          "name": "moment",
          "dependency": "moment",
          "title": "Regular Expression Denial of Service in moment",
          "url": "https://github.com/advisories/GHSA-446m-mv8f-q348",
          "severity": "high",
          "cwe": [
            "CWE-400"
          ],
          "cvss": {
            "score": 7.5,
            "vectorString": "CVSS:3.1/AV:N/AC:L/PR:N/UI:N/S:U/C:N/I:N/A:H"
          },
          "range": "<2.19.3"
        },
        {
          "source": 1109571,
          "name": "moment",
          "dependency": "moment",
          "title": "Path Traversal: 'dir/../../filename' in moment.locale",
          "url": "https://github.com/advisories/GHSA-8hfj-j24r-96c4",
          "severity": "high",
          "cwe": [
            "CWE-22",
            "CWE-27"
          ],
          "cvss": {
            "score": 7.5,
            "vectorString": "CVSS:3.1/AV:N/AC:L/PR:N/UI:N/S:U/C:N/I:H/A:N"
          },
          "range": "<2.29.2"
        }
      ],
      "effects": [],
      "range": "<=2.29.1",
      "nodes": [
        "node_modules/moment"
      ],
      "fixAvailable": {
        "name": "moment",
        "version": "2.30.1",
        "isSemVerMajor": false
      }
    },
    "rollup": {
      "name": "rollup",
      "severity": "high",
      "isDirect": false,
      "via": [
        {
          "source": 1113515,
          "name": "rollup",
          "dependency": "rollup",
          "title": "Rollup 4 has Arbitrary File Write via Path Traversal",
          "url": "https://github.com/advisories/GHSA-mw96-cpmx-2vgc",
          "severity": "high",
          "cwe": [
            "CWE-22"
          ],
          "cvss": {
            "score": 0,
            "vectorString": null
          },
          "range": ">=4.0.0 <4.59.0"
        }
      ],
      "effects": [],
      "range": "4.0.0 - 4.58.0",
      "nodes": [
        "node_modules/rollup"
      ],
      "fixAvailable": true
    },
    "vite": {
      "name": "vite",
      "severity": "moderate",
      "isDirect": true,
      "via": [
        {
          "source": 1107324,
          "name": "vite",
          "dependency": "vite",
          "title": "Vite middleware may serve files starting with the same name with the public directory",
          "url": "https://github.com/advisories/GHSA-g4jq-h2w9-997c",
          "severity": "low",
          "cwe": [
            "CWE-22",
            "CWE-200",
            "CWE-284"
          ],
          "cvss": {
            "score": 0,
            "vectorString": null
          },
          "range": ">=6.0.0 <=6.3.5"
        },
        {
          "source": 1107328,
          "name": "vite",
          "dependency": "vite",
          "title": "Vite's `server.fs` settings were not applied to HTML files",
          "url": "https://github.com/advisories/GHSA-jqfw-vq24-v9c3",
          "severity": "low",
          "cwe": [
            "CWE-23",
            "CWE-200",
            "CWE-284"
          ],
          "cvss": {
            "score": 0,
            "vectorString": null
          },
          "range": ">=6.0.0 <=6.3.5"
        },
        {
          "source": 1109135,
          "name": "vite",
          "dependency": "vite",
          "title": "vite allows server.fs.deny bypass via backslash on Windows",
          "url": "https://github.com/advisories/GHSA-93m4-6634-74q7",
          "severity": "moderate",
          "cwe": [
            "CWE-22"
          ],
          "cvss": {
            "score": 0,
            "vectorString": null
          },
          "range": ">=6.0.0 <=6.4.0"
        }
      ],
      "effects": [],
      "range": "6.0.0 - 6.4.0",
      "nodes": [
        "node_modules/vite"
      ],
      "fixAvailable": {
        "name": "vite",
        "version": "6.4.1",
        "isSemVerMajor": false
      }
    }
  },
  "metadata": {
    "vulnerabilities": {
      "info": 0,
      "low": 0,
      "moderate": 1,
      "high": 3,
      "critical": 0,
      "total": 4
    },
    "dependencies": {
      "prod": 41,
      "dev": 73,
      "optional": 49,
      "peer": 0,
      "peerOptional": 0,
      "total": 113
    }
  }
}
```
