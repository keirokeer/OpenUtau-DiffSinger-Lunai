import argparse
from datetime import datetime

def main():
    parser = argparse.ArgumentParser(description='Generate Appcast XML')
    parser.add_argument('-v', '--version', required=True, help='Version number')
    parser.add_argument('-o', '--os', required=True, help='OS name (windows/macos/linux)')
    parser.add_argument('-r', '--rid', required=True, help='Runtime identifier')
    parser.add_argument('-f', '--file', required=True, help='File name')
    args = parser.parse_args()

    xml = f'''<?xml version="1.0" encoding="utf-8"?>
<rss version="2.0" xmlns:sparkle="http://www.andymatuschak.org/xml-namespaces/sparkle">
<channel>
    <title>OpenUtau</title>
    <language>en</language>
    <item>
    <title>OpenUtau {args.version}</title>
    <pubDate>{datetime.now().strftime("%a, %d %b %Y %H:%M:%S %z")}</pubDate>
    <enclosure url="https://github.com/keirokeer/OpenUtau-DiffSinger-Lunai/releases/download/{args.version}/{args.file}"
                sparkle:version="{args.version}"
                sparkle:shortVersionString="{args.version}"
                sparkle:os="{args.os}"
                type="application/octet-stream"
                sparkle:signature="" />
    </item>
</channel>
</rss>'''

    output_file = f"appcast.{args.rid}.xml"
    with open(output_file, 'w') as f:
        f.write(xml)
    print(f"Generated {output_file} for {args.file}")

if __name__ == '__main__':
    main()
