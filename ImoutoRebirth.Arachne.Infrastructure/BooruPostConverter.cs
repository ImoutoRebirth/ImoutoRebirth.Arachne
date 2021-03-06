﻿using System.Collections.Generic;
using System.Linq;
using Imouto.BooruParser.Model.Danbooru.Json;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.Models;
using ImoutoRebirth.Common;
using Note = ImoutoRebirth.Arachne.Core.Models.Note;
using Post = Imouto.BooruParser.Model.Base.Post;
using SearchResult = ImoutoRebirth.Arachne.Core.Models.SearchResult;
using Tag = ImoutoRebirth.Arachne.Core.Models.Tag;

namespace ImoutoRebirth.Arachne.Infrastructure
{
    internal class BooruPostConverter : IBooruPostConverter
    {
        public SearchResult Convert(
            Post post, 
            Image image, 
            SearchEngineType searchEngineType)
        {
            var metadataParsingDto = ToMetadataParsingResults(post);

            var metaTags = metadataParsingDto
                          .GetMetaTags()
                          .Select(x => new Tag(x.TagType, x.Tag, x.Value));
            var postTags = metadataParsingDto
                          .Tags
                          .Select(x => new Tag(x.TagType, x.Tag, x.Value, x.Synonyms));

            return new Metadata(
                image, 
                searchEngineType, 
                true, 
                metaTags.Union(postTags).ToArray(), 
                metadataParsingDto.Notes,
                post.PostId);
        }

        private static MetaParsingResults ToMetadataParsingResults(Post post)
        {
            var (parentId, parentMd5) = GetParentInfo(post.ParentId);
            
            return new MetaParsingResults(
                post.Source,
                post.PostId.ToString(),
                post.ActualDateTime.ToString(Constants.DateTimeFormat),
                post.Md5,
                post.PostedDateTime.ToLongTimeString(),
                post.PostedUser?.Id?.ToString(),
                post.PostedUser?.Name,
                post.ImageRating.ToString(),
                parentId,
                parentMd5,
                post.ChildrenIds,
                post.Pools.Select(x => x.Id + "||" + x.Name).ToArray(),
                post.Tags.Select(ConvertTag).ToArray(),
                post.Notes.Select(ConvertNote).ToArray(),
                post.UgoiraFrameData);
        }

        private static (string, string) GetParentInfo(string postParentId)
        {
            var (parentId, parentMd5) = ((string)null, (string)null);

            if (!string.IsNullOrWhiteSpace(postParentId))
            {
                var parts = postParentId.Split(':');
                parentId = parts[0];

                if (parts.Length > 1)
                    parentMd5 = parts[1];
            }

            return (parentId, parentMd5);
        }

        private static MetaParsingTagResults ConvertTag(Imouto.BooruParser.Model.Base.Tag tag)
            => new MetaParsingTagResults(tag.Type.ToString(), tag.Name, new[] {tag.JapName}, null);
        
        private static Note ConvertNote(Imouto.BooruParser.Model.Base.Note note) 
            => new Note(
                note.Label, 
                new NotePosition(
                    note.NotePoint.Top, 
                    note.NotePoint.Left, 
                    note.NoteSize.Width, 
                    note.NoteSize.Height),
                note.Id);
    }
}