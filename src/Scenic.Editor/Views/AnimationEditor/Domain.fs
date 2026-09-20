namespace Scenic.Editor.Views.AnimationEditor

open System.Numerics
open SharpGLTF.Schema2
open Silk.NET.Maths

module Domain =
    

    type Bone = {
        NodeId: int
        Name: string
        Children: Bone list
    }
    
    type Joint =
        {
            JointIndex: int
            NodeIndex: int
            InverseBindMatrix: Matrix4x4
            DefaultTranslation: Vector3
            DefaultRotation: Quaternion
            DefaultScale: Vector3
        }
        
        static member Default =
            {
                JointIndex = -1
                NodeIndex = -1
                InverseBindMatrix = Matrix4x4.Identity
                DefaultTranslation = Vector3.Zero
                DefaultRotation = Quaternion.Zero
                DefaultScale = Vector3.One
            }

    type Armature = {
        RootBones: Bone list
        Joints: Map<int, Joint>
    }
    
    type Keyframe<'T> = {Time: float32; Value: 'T}
    
    type TranslationKeyFrame = { Time: float32; Value: Vector3 }

    type RotationKeyFrame = { Time: float32; Value: Quaternion }

    type ScaleKeyFrame = { Time: float32; Value: Vector3 }

    type Channel =
        { NodeId: int
          TranslationKeyframes: Keyframe<Vector3> array
          RotationKeyframes: Keyframe<Quaternion> array
          ScaleKeyframes: Keyframe<Vector3> array }

    type AnimationClip =
        { Name: string
          Duration: float32
          Channels: Map<int, Channel> }

    
    type Animator() =
        let mutable currentAnimation: AnimationClip option = None
        let mutable currentClipPosition = 0f
        
        
        member _.PlayAnimation(clip: AnimationClip) =
            currentAnimation <- Some clip
            
            
            
            
            ()
        
        
        

    module Operations =
        
        let loadAnimations (modelRoot: ModelRoot) =
            [ for animation in modelRoot.LogicalAnimations do
              { Name = animation.Name
                Duration = animation.Duration
                Channels =
                  [ for (nodeId, channels) in animation.Channels |> Seq.groupBy (fun c -> c.TargetNode.LogicalIndex) do
                        let tc, rc, sc =
                            channels
                            |> Seq.fold
                                (fun
                                    (tc: Keyframe<Vector3> array option,
                                     rc: Keyframe<Quaternion> array option,
                                     sc: Keyframe<Vector3> array option)
                                    c ->
                                    let newTc =
                                        match c.GetTranslationSampler() with
                                        | null -> tc
                                        | sampler ->
                                            [| for time, v in sampler.GetLinearKeys() do
                                                   { Time = time; Value = v }: Keyframe<Vector3> |]
                                            |> Some

                                    let newRc =
                                        match c.GetRotationSampler() with
                                        | null -> rc
                                        | sampler ->
                                            [| for time, v in sampler.GetLinearKeys() do
                                                   ({ Time = time; Value = v }: Keyframe<Quaternion>) |]
                                            |> Some

                                    let newSc =
                                        match c.GetScaleSampler() with
                                        | null -> sc
                                        | sampler ->
                                            [| for time, v in sampler.GetLinearKeys() do
                                                   ({ Time = time; Value = v }: Keyframe<Vector3>) |]
                                            |> Some

                                    (newTc, newRc, newSc))
                                (None, None, None)

                        (nodeId,
                        { NodeId = nodeId
                          TranslationKeyframes = tc |> Option.defaultValue [||]
                          RotationKeyframes = rc |> Option.defaultValue [||]
                          ScaleKeyframes = sc |> Option.defaultValue [||] }) ]
                  |> Map.ofList } ]
        
        let loadArmature (modelRoot: ModelRoot) =
            [ for skin in modelRoot.LogicalSkins do
              let joints = skin.Joints
              
              let j = ResizeArray<Joint>()
              

              let root =
                  joints
                  |> Seq.filter (fun j ->
                      let hasParent =
                          joints |> Seq.exists (fun other -> other.VisualChildren |> Seq.contains j)

                      hasParent |> not)
                  
              let i = skin.InverseBindMatrices
              
              let jointIndexMap = skin.Joints |> Seq.mapi (fun idx node -> node.LogicalIndex, idx) |> Map.ofSeq

              let rec build (node: Node) =
                  
                  let iv = i.[node.LogicalIndex]
                  
                  let jointIdx = jointIndexMap.[node.LogicalIndex] 

                  j.Add({
                      JointIndex = jointIdx
                      NodeIndex = node.LogicalIndex
                      InverseBindMatrix = i.[jointIdx]
                      DefaultTranslation = node.LocalTransform.Translation
                      DefaultRotation = node.LocalTransform.Rotation
                      DefaultScale = node.LocalTransform.Scale
                  }: Joint)
                  
                 
                  
                  { NodeId = node.LogicalIndex
                    Name = node.Name
                    Children = node.VisualChildren |> Seq.map build |> List.ofSeq }
                  
              let rootBones = root |> Seq.map build |> List.ofSeq

              { RootBones = rootBones
                Joints = j |> Seq.map (fun j -> j.NodeIndex, j) |> Map.ofSeq } ]
        
        let sampleChannel<'T>
            (keyframes: Keyframe<'T> array)
            (time: float32)
            (interpolate: 'T * 'T * float32 -> 'T)
            =
            //if keyframes.Length = 0 then defaultValue
            let mutable i = 0
            
            while i < keyframes.Length - 1 && time > keyframes.[i + 1].Time do
                i <- i + 1
                
            let (ki0, ki1) = if i = keyframes.Length - 1 then i, 0 else i, i + 1
            
            let k0 = keyframes[ki0]
            let k1 = keyframes[ki1]
            
            let t = (time - k0.Time) / (k1.Time - k0.Time)
            
            interpolate (k0.Value, k1.Value, t)
            
        let calculateLocalMatrix (nodeIndex: int) (channels: Map<int, Channel>) (time: float32) (joint: Joint) =
            match Map.tryFind nodeIndex channels with
            | None ->
                // From previous code. This is probably correct.
                //Matrix4x4.Identity
                //* Matrix4x4.CreateFromQuaternion(joint.DefaultRotation)
                //* Matrix4x4.CreateScale(joint.DefaultScale)
                //* Matrix4x4.CreateTranslation(joint.DefaultTranslation)
                
                Matrix4x4.CreateScale(joint.DefaultScale) *
                Matrix4x4.CreateFromQuaternion(joint.DefaultRotation) *
                Matrix4x4.CreateTranslation(joint.DefaultTranslation)
            | Some channel ->
                let t = sampleChannel channel.TranslationKeyframes time Vector3.Lerp
                let r = sampleChannel channel.RotationKeyframes time Quaternion.Slerp
                let s = sampleChannel channel.ScaleKeyframes time Vector3.Lerp
                
                // System.Numerics is Row-Major, so S * R * T is correct for local layout
                Matrix4x4.CreateScale(s) * 
                Matrix4x4.CreateFromQuaternion(r) * 
                Matrix4x4.CreateTranslation(t)
                
                
                //Matrix4x4.CreateFromQuaternion(r) * 
                //Matrix4x4.CreateScale(s) * 
                //Matrix4x4.CreateTranslation(t)
                
                
        let rec computeGlobalMatrices
            (nodeIndex: int)
            (parentTransform: Matrix4x4)
            (channels: Map<int, Channel>)
            (time: float32)
            (joints: Map<int, Joint>)
            (bone: Bone)
            (globalMatrics: outref<Map<int, Matrix4x4>>)
            =
            
                let joint = joints |> Map.tryFind nodeIndex |> Option.defaultValue Joint.Default
                
                let localMatrix = calculateLocalMatrix nodeIndex channels time joint
                
                let globalMatrix = localMatrix * parentTransform
                //let final = joint.InverseBindMatrix * globalMatrix
                
                
                //let globalMatrix = parentTransform * localMatrix
                let final = joint.InverseBindMatrix * globalMatrix 
                globalMatrics <- Map.add joint.JointIndex final globalMatrics
                
                
                for child in bone.Children do
                    computeGlobalMatrices child.NodeId globalMatrix channels time joints child &globalMatrics
                    
        let generateShaderPalette
            (armature: Armature)
            (channels: Map<int, Channel>)
            (time: float32)
            (joints: Map<int, Joint>) =
            
            let mutable globalMatrices: Map<int, Matrix4x4> = Map.empty
            
            for rootBone in armature.RootBones do
                computeGlobalMatrices rootBone.NodeId Matrix4x4.Identity channels time joints rootBone &globalMatrices
            
            joints.Values
            |> Array.ofSeq
            |> Array.sortBy (fun j -> j.JointIndex)
            |> Array.map (fun joint ->
                match globalMatrices.TryFind joint.JointIndex with
                | None -> Matrix4x4.Identity
                | Some globalMatrix -> (*joint.InverseBindMatrix **) globalMatrix)
            
            