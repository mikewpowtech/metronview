using System;
using System.Linq;
using System.Collections.Generic;

namespace Metron2Configuration
{
    public class ChannelsConfiguration : Dictionary<int, ChannelConfigurationList>, IVisitableConfiguration
    {
        public void Merge(ChannelsConfiguration toMerge)
        {
            foreach (KeyValuePair<int, ChannelConfigurationList> pair in toMerge)
            {
                // If the value is blank, nothing to do.
                if (null == pair.Value)
                    continue;

                // If this is the first time we've seen anything for this channel, grab it wholesale.
                if (!this.ContainsKey(pair.Key))
                {
                    this[pair.Key] = pair.Value;
                    continue;
                }

                // TODO: For neatness, move this logic into ChannelConfigurationList
                // If we get here, we know both the source and destination have something for this channel; we need to merge or replace.
                // Check whether the source and destination are mergeable.  Rules:
                // - If neither has a primary, just accumulate the secondaries and hope that a primary comes along sometime.
                // - If both have primaries, the new ones overwrites the old one.
                // - Otherwise, use the primary that exists.
                // These rules can be coded as: if there's a new primary, remove any old primary and add the new primary; otherwise do nothing.
                ChannelConfiguration sourcePrimary = pair.Value.FirstOrDefault(c => c.IsPrimaryConfiguration);
                ChannelConfigurationList destinationList = this[pair.Key];
                ChannelConfiguration destinationPrimary = destinationList.FirstOrDefault(c => c.IsPrimaryConfiguration);
                if (null != sourcePrimary)
                {
                    if (null != destinationPrimary)
                        destinationList.Remove(destinationPrimary);
                    destinationList.Add(sourcePrimary);
                    destinationPrimary = sourcePrimary;
                }
                // Once the primary is known, discard any secondaries with no types compatible with that primary.
                if (null != destinationPrimary)
                {
                    PrimaryChannelType primaryType = destinationPrimary.CompatibleChannelTypes[0];
                    List<ChannelConfiguration> toRemove = new List<ChannelConfiguration>();
                    foreach (ChannelConfiguration candidate in destinationList)
                    {
                        if (!candidate.IsPrimaryConfiguration && !candidate.CompatibleChannelTypes.Contains(primaryType))
                            toRemove.Add(candidate);
                    }
                    foreach (ChannelConfiguration victim in toRemove)
                        destinationList.Remove(victim);
                }
                // Now we're safe to merge in new compatible secondaries
                destinationList.AddRange(pair.Value.Where(c => !c.IsPrimaryConfiguration && (null == destinationPrimary || c.CompatibleChannelTypes.Contains(destinationPrimary.CompatibleChannelTypes[0]))));
            }
        }

        public void Accept(IConfigurationVisitor visitor)
        {
            visitor.Visit(this);
            int[] sortedChannels = Keys.ToArray();
            Array.Sort(sortedChannels);
            foreach (int channel in sortedChannels)
            {
                ChannelConfigurationList victims;
                if (null != this[channel] && TryGetValue(channel, out victims))
                    victims.Accept(visitor);
            }
        }
    }
}
