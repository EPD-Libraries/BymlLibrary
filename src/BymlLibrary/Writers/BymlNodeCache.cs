using System.Runtime.CompilerServices;

namespace BymlLibrary.Writers;

public class BymlNodeCache
{
    private readonly Dictionary<Byml, (int Hash, int Bucket)> _hashes = [];
    private readonly Dictionary<BymlNodeType, List<Dictionary<int, (Byml Node, int? Offset)>>> _storage = [];

    public int this[Byml node] {
        set {
            if (!_storage.TryGetValue(node.Type, out var buckets)) {
                _storage[node.Type] = buckets = [
                    new() {
                        { value, (node, null) }
                    }
                ];

                _hashes.Add(node, (value, 0));
                return;
            }

            int index = -1;
            while (!GetBucket(ref buckets, ++index).TryAdd(value, (node, null))) {
                // If the colliding node matches
                // leave it as is
                if (Byml.ValueEqualityComparer.Default.Equals(node, buckets[index][value].Node)) {
                    goto Update;
                }
            }

        Update:
            _hashes.Add(node, (value, index));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateOffset(int hash, int bucket, Byml node, int offset)
    {
        _storage[node.Type][bucket][hash] = (node, offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int? Lookup(Byml node, out int hash, out int bucket)
    {
        (hash, bucket) = _hashes[node];
        return _storage[node.Type][bucket][hash].Offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Dictionary<int, (Byml, int?)> GetBucket(ref List<Dictionary<int, (Byml, int?)>> buckets, int index)
    {
        if (index >= buckets.Count) {
            buckets.Add([]);
        }

        return buckets[index];
    }
}
